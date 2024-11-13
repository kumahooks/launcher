using System.Diagnostics;

namespace LaunchInfoDumper.Models
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Path { get; set; }
        public DateTime StartTime { get; set; }
        public string UserName { get; set; }
        public string UserIdentity { get; set; }
        public string WorkingDirectory {  get; set; }
        public Boolean IsElevated { get; set; }
        public TimeSpan CpuTime { get; set; }
        public long MemoryUsageMB { get; set; }
        public int Priority { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public string CommandLine { get; set; }
        public string WindowTitle { get; set; }
        public List<ModuleInfo> LoadedModules { get; set; } = new();
        public List<ThreadInfo> Threads { get; set; } = new();

        public static ProcessInfo FromProcess(Process process)
        {
            return new ProcessInfo
            {
                Name = process.ProcessName,
                Id = process.Id,
                Path = process.MainModule?.FileName,
                StartTime = process.StartTime,
                CpuTime = process.TotalProcessorTime,
                MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
                Priority = process.BasePriority,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                WindowTitle = process.MainWindowTitle
            };
        }
    }

    public class ModuleInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
    }

    public class ThreadInfo
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public System.Diagnostics.ThreadState State { get; set; }
        public TimeSpan CpuTime { get; set; }
    }
}