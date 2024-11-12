using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace LauncherApp
{
    class LauncherSettings
    {
        public string path { get; set; } = "C:\\Windows\\system32\\calc.exe";
        public string arguments { get; set; } = "";
    }

    class Program
    {
        private const string settingsFileName = "launcher_settings.json";

        static void Main()
        {
            try {
                ensureSettingsExist();
                LauncherSettings settings = LoadSettings();
                if (string.IsNullOrEmpty(settings.path)) {
                    Console.Error.WriteLine("Error: 'ApplicationPath' must be specified in the configuration file.");
                    return;
                }

                LaunchApplication(settings);
            } catch (Exception ex) {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ensureSettingsExist()
        {
            if (!File.Exists(settingsFileName)) {
                var defaultSettings = new LauncherSettings {
                    path = "C:\\Windows\\system32\\calc.exe",
                    arguments = ""
                };

                string defaultJson = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFileName, defaultJson);

                Console.WriteLine($"Configuration file '{settingsFileName}' was not found.");
                Console.WriteLine("A default configuration file has been created. Please edit it with the correct application path and arguments.");
            }
        }

        private static LauncherSettings LoadSettings()
        {
            if (!File.Exists(settingsFileName)) {
                throw new FileNotFoundException($"Settings file '{settingsFileName}' not found.");
            }

            string jsonData = File.ReadAllText(settingsFileName);
            if (string.IsNullOrEmpty(jsonData)) {
                throw new InvalidOperationException("Configuration file is empty.");
            }

            var settings = JsonSerializer.Deserialize<LauncherSettings>(jsonData);
            if (settings == null) {
                throw new InvalidOperationException("Failed to deserialize the configuration file.");
            }

            return settings;
        }

        private static void LaunchApplication(LauncherSettings settings)
        {
            var startInfo = new ProcessStartInfo {
                FileName = settings.path,
                Arguments = settings.arguments ?? "",
                UseShellExecute = true
            };

            Process process = Process.Start(startInfo);
            if (process != null) {
                Console.Write($"Launched application: {settings.path}");
                Console.WriteLine(!string.IsNullOrEmpty(settings.arguments) ? $" with arguments: \"{settings.arguments}\"" : "");
            } else {
                Console.Error.WriteLine("Error: Failed to start the application.");
            }
        }
    }
}
