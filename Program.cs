using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0) {
            Console.WriteLine("Please provide the path to the application as an argument.");
            return;
        }

        string applicationPath = args[0];

        try
        {
            Process.Start(applicationPath);
            Console.WriteLine($"Launched application: {applicationPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error launching application: {ex.Message}");
        }
    }
}
