using System;
using System.Diagnostics;
using System.Security;
using System.Threading;

namespace Common
{
    using static Utilities;

    public static class OpenVPN
    {
        public static bool StartOpenVPN(string _config, string _serviceName, string _userName, SecureString _password)
        {
            var command = $"openvpn --config {_config} --service {_serviceName} 0";

            Process process = null;
            ProcessStartInfo processInfo = null;

            Debug.WriteLine("Command = " + command);
            Debug.WriteLine("");

            Console.WriteLine(Divider);
            Console.WriteLine("OpenVPN" + Divider);
            Console.WriteLine(Divider);
            Console.WriteLine("Username: " + _userName);
            Console.WriteLine("Password: " + _password);
            Console.WriteLine();
            Console.Write("Trying...");

            var success = false;
            try
            {
                processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };

                process = new Process {StartInfo = processInfo};
                process.Start();

                process.StandardInput.WriteLine(_userName);
                Thread.Sleep(20);

                process.StandardInput.WriteLine(_password.ToString());
                Thread.Sleep(20);

                var ticks = 0;
                while (!success && !process.HasExited && ticks < 100)
                {
                    var line = process.StandardOutput.ReadLine();

                    Debug.WriteLine($">>{line}");

                    if (line.Contains("Initialization Sequence Completed"))
                        success = true;

                    ticks++;
                }

                Console.WriteLine(success ? "...Success" : "...Failed");

                Console.WriteLine(Divider);
                Console.WriteLine(Divider);
                Console.WriteLine(Divider);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Opening : " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                process.Close();
                process = null;
            }

            return success;
        }
    }
}