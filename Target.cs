using System.Diagnostics;

namespace ExecutionInfoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Program Executed: {AppDomain.CurrentDomain.FriendlyName}");

            if (args.Length == 0) {
                Console.WriteLine("No arguments were passed.");
            } else {
                string arguments = string.Join(", ", args);
                Console.WriteLine($"Arguments: {arguments}");
            }

            DateTime startTime = Process.GetCurrentProcess().StartTime;
            Console.WriteLine($"App started at: {startTime}");

            string userName = Environment.UserName;
            Console.WriteLine($"App launched by user: {userName}");

            string executingPath = Environment.CurrentDirectory;
            Console.WriteLine($"App was executed from: {executingPath}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
