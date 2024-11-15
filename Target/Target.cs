using LaunchInfoDumper.Models;
using LaunchInfoDumper.Services;
using System.Text;

namespace LaunchInfoDumper
{
    class Target
    {
        static async Task Main(string[] args)
        {
            var analyzer = new ProcessAnalyzer();
            ProcessInfo currentProcess = null;
            ProcessInfo parentProcess = null;
            Dictionary<string, string> envVars = null;
            List<ProcessInfo> siblings = null;

            currentProcess = analyzer.GetCurrentProcessInfo(args);
            parentProcess = analyzer.GetParentProcessInfo();
            envVars = analyzer.GetEnvironmentVariables();
            siblings = analyzer.GetSiblingProcesses();

            Console.Write(CreateProcessInfoDump("Current Process", currentProcess));
            if (parentProcess != null) Console.Write(CreateProcessInfoDump("Parent Process", parentProcess));
            DisplayEnvironmentVariables(envVars);
            DisplaySiblingProcesses(siblings);

            Console.WriteLine($"{Environment.NewLine}Press 'Q' to exit, 'W' to write dump to file...");
            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Q) break;
                if (key == ConsoleKey.W)
                {
                    await WriteDumpToFile(currentProcess, parentProcess, envVars, siblings);
                    Console.WriteLine("Process information written to process_dump.txt");
                }
                await Task.Delay(100);
            }
        }

        static string CreateProcessInfoDump(string header, ProcessInfo process)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"=== {header} ===");

            builder.AppendLine($"- Name: {process.Name}");
            builder.AppendLine($"- ID: {process.Id}");
            builder.AppendLine($"- Path: {process.Path}");
            builder.AppendLine($"- Start Time: {process.StartTime}");
            builder.AppendLine($"- Is Elevated: {process.IsElevated}");

            if (!string.IsNullOrEmpty(process.UserName)) 
                builder.AppendLine($"- User Name: {process.UserName}");

            if (!string.IsNullOrEmpty(process.UserIdentity))
                builder.AppendLine($"- User Identity: {process.UserIdentity}");

            if (!string.IsNullOrEmpty(process.WorkingDirectory))
                builder.AppendLine($"- Working Directory: {process.WorkingDirectory}");

            if (!string.IsNullOrEmpty(process.CommandLine))
                builder.AppendLine($"- Command Line: {process.CommandLine}");

            if (!string.IsNullOrEmpty(process.WindowTitle))
                builder.AppendLine($"- Window Title: {process.WindowTitle}");

            if (process.CpuTime != TimeSpan.Zero)
            {
                builder.AppendLine($"- CPU Time: {process.CpuTime}");
                builder.AppendLine($"- Memory Usage: {process.MemoryUsageMB} MB");
                builder.AppendLine($"- Priority: {process.Priority}");
                builder.AppendLine($"- Thread Count: {process.ThreadCount}");
                builder.AppendLine($"- Handle Count: {process.HandleCount}");
            }

            if (process.LoadedModules.Any())
            {
                builder.AppendLine($"{Environment.NewLine}- Loaded Modules:");
                foreach (var module in process.LoadedModules)
                {
                    builder.AppendLine($"-- {module.Name}: \"{module.FileName}\"");
                }
            }

            if (process.Threads.Any())
            {
                builder.AppendLine($"{Environment.NewLine}- Thread Information:");
                foreach (var thread in process.Threads)
                {
                    builder.AppendLine($"-- Thread ID: {thread.Id}");
                    builder.AppendLine($"-- Priority: {thread.Priority}");
                    builder.AppendLine($"-- State: {thread.State}");
                    builder.AppendLine($"-- CPU Time: {thread.CpuTime}");
                }
            }

            builder.AppendLine();

            return builder.ToString();
        }

        static void DisplayEnvironmentVariables(Dictionary<string, string> envVars)
        {
            Console.WriteLine($"{Environment.NewLine}=== Environment Variables ===");
            foreach (var entry in envVars)
            {
                Console.WriteLine($"- {entry.Key} = \"{entry.Value}\"");
            }

            Console.WriteLine();
        }
    
        static void DisplaySiblingProcesses(List<ProcessInfo> siblings)
        {
            Console.WriteLine($"{Environment.NewLine}=== Sibling Processes ===");
            foreach (var sibling in siblings)
            {
                Console.WriteLine($"- {sibling.Name} (ID: {sibling.Id})");
            }

            Console.WriteLine();
        }

        static async Task WriteDumpToFile(
            ProcessInfo currentProcess,
            ProcessInfo parentProcess,
            Dictionary<string, string> envVars,
            List<ProcessInfo> siblings)
        {
            using var writer = new StreamWriter("process_dump.txt", false);
            await writer.WriteLineAsync($"Date: {DateTime.Now}{Environment.NewLine}");

            // Write current process info
            await writer.WriteLineAsync(CreateProcessInfoDump("CURRENT PROCESS", currentProcess));

            // Write parent process info
            if (parentProcess != null)
                await writer.WriteLineAsync(CreateProcessInfoDump("PARENT PROCESS", parentProcess));

            // Write environment variables
            await writer.WriteLineAsync($"{Environment.NewLine}=== ENVIRONMENT VARIABLES ===");
            foreach (var entry in envVars)
            {
                await writer.WriteLineAsync($"({entry.Key}) = ({entry.Value})");
            }

            // Write sibling processes
            if (siblings.Count > 0)
            {
                await writer.WriteLineAsync($"{Environment.NewLine}=== SIBLING PROCESSES ===");
                foreach (var sibling in siblings)
                {
                    await writer.WriteLineAsync($"{sibling.Name} (ID: {sibling.Id})");
                }
            }
        }
    }
}