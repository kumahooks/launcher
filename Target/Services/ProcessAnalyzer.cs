using System.Diagnostics;
using System.Security.Principal;
using System.Management;
using LaunchInfoDumper.Models;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LaunchInfoDumper.Services
{
    public class ProcessAnalyzer
    {
        public ProcessInfo GetCurrentProcessInfo(string[] args)
        {
            var currentProcess = Process.GetCurrentProcess();
            return new ProcessInfo
            {
                Name = AppDomain.CurrentDomain.FriendlyName,
                Id = currentProcess.Id,
                Path = currentProcess.MainModule?.FileName,
                StartTime = currentProcess.StartTime,
                CommandLine = GetProcessCommandLine(currentProcess.Id),
                UserName = Environment.UserName,
                UserIdentity = WindowsIdentity.GetCurrent().Name,
                WorkingDirectory = Environment.CurrentDirectory,
                IsElevated = IsProcessElevated()
            };
        }

        public Dictionary<string, string> GetEnvironmentVariables()
        {
            return Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(e => e.Key.ToString(), e => e.Value?.ToString());
        }

        public ProcessInfo GetParentProcessInfo()
        {
            var currentProcess = Process.GetCurrentProcess();
            var parentProcessId = GetParentProcessId(currentProcess.Handle);

            try
            {
                var parentProcess = Process.GetProcessById(parentProcessId);
                var info = ProcessInfo.FromProcess(parentProcess);
                info.CommandLine = GetProcessCommandLine(parentProcessId);
                info.IsElevated = IsProcessElevated();

                // Add modules
                try
                {
                    foreach (ProcessModule module in parentProcess.Modules)
                    {
                        info.LoadedModules.Add(new ModuleInfo
                        {
                            Name = module.ModuleName,
                            FileName = module.FileName
                        });
                    }
                }
                catch (Win32Exception)
                {
                    // Module access denied
                }

                // Add threads
                foreach (ProcessThread thread in parentProcess.Threads)
                {
                    info.Threads.Add(new ThreadInfo
                    {
                        Id = thread.Id,
                        Priority = thread.CurrentPriority,
                        State = thread.ThreadState,
                        CpuTime = thread.TotalProcessorTime
                    });
                }

                return info;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<ProcessInfo> GetSiblingProcesses()
        {
            var currentProcess = Process.GetCurrentProcess();
            var parentId = GetParentProcessId(currentProcess.Handle);
            return GetChildProcesses(parentId)
                .Where(p => p.Id != currentProcess.Id)
                .Select(ProcessInfo.FromProcess)
                .ToList();
        }

        private string GetProcessCommandLine(int processId)
        {
            using var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            using var objects = searcher.Get();
            var obj = objects.Cast<ManagementObject>().FirstOrDefault();
            return obj?["CommandLine"]?.ToString() ?? "Not available";
        }

        private List<Process> GetChildProcesses(int parentId)
        {
            var children = new List<Process>();
            var snapshot = Win32API.CreateToolhelp32Snapshot(Win32API.TH32CS_SNAPPROCESS, 0);

            try
            {
                var processEntry = new Win32API.PROCESSENTRY32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(Win32API.PROCESSENTRY32))
                };

                if (Win32API.Process32First(snapshot, ref processEntry))
                {
                    do
                    {
                        if (processEntry.th32ParentProcessID == parentId)
                        {
                            try
                            {
                                children.Add(Process.GetProcessById((int)processEntry.th32ProcessID));
                            }
                            catch (ArgumentException) { }
                        }
                    } while (Win32API.Process32Next(snapshot, ref processEntry));
                }
            }
            finally
            {
                Win32API.CloseHandle(snapshot);
            }

            return children;
        }

        private int GetParentProcessId(IntPtr handle)
        {
            var pbi = new Win32API.ProcessBasicInformation();
            var returnLength = 0;

            var status = Win32API.NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
            {
                return -1;
            }

            return pbi.InheritedFromUniqueProcessId.ToInt32();
        }

        private bool IsProcessElevated(Process process = null)
        {
            try
            {
                using var identity = process != null
                    ? new WindowsIdentity(process.Handle)
                    : WindowsIdentity.GetCurrent();

                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
