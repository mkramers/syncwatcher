using System;
using System.Diagnostics;

namespace Common
{
    public class ExecutableInfo
    {
        public string Executable { get; set; }
        public string Args { get; set; }
        public string WorkingDirectory { get; set; }

        public string Command
        {
            get
            {
                string command = $"{Executable}";
                return command;
            }
        }

        public static void ExecuteCommand(ExecutableInfo _info)
        {
            Debug.WriteLine($"Execucting: {_info.Command}");

            //var processInfo = new ProcessStartInfo("cmd.exe", "/c " + _info.Command);
            ProcessStartInfo processInfo = new ProcessStartInfo("openvpn.exe", _info.Command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            //processInfo.WorkingDirectory = _info.WorkingDirectory;

            Process process = new Process
            {
                StartInfo = processInfo
            };

            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    }
}
