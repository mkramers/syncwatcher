using System;
using System.Diagnostics;
using System.IO;

namespace P4Commands
{
    public class P4Executable
    {
        public void Run(string[] _args)
        {
            Debug.Assert(_args != null);

            if (_args.Length >= 2)
            {
                var operationArgs = _args.SubArray(1, _args.Length - 1);

                var operation = _args[0].ToLower();
                switch (operation)
                {
                    case "-cl":
                        if (!TryGetNewChangelist(operationArgs))
                        {
                            DisplayError("Failed to create changelist!");
                        }
                        break;
                    case "-next":
                        if (!TryWriteNextVersionFromFile(operationArgs))
                        {
                            DisplayError("Failed to get new version!");
                        }
                        break;
                    case "-getfrombuildnumber":
                        if (!TryWriteNextVersionFromFile2(operationArgs))
                        {
                            DisplayError("Failed to get new version!");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                DisplayError("Insufficient args");
                DisplayUsage();
            }
        }

        public void DisplayError(string _message)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(_message));
            Console.Write($"Error: {_message}");
        }

        public void DisplayUsage()
        {

        }

        //helpers

        private static bool TryGetNewChangelist(string[] _args)
        {
            Debug.Assert(_args != null);

            if (_args.Length < 2)
            {
                Console.WriteLine("Insufficient args in TryGetNewChangelist");
                return false;
            }

            var description = _args[0];
            var outFile = _args[1];

            if (_args.Length > 3)
            {
                Console.WriteLine("Warning: Extra args in TryGetNewChangelist");
            }

            var success = Commands.TryWriteChangelistToFile(description, outFile, out var _);
            return success;
        }

        private static bool TryWriteNextVersionFromFile(string[] _args)
        {
            Debug.Assert(_args != null);

            if (_args.Length < 3)
            {
                Console.WriteLine("Insufficient args in TryWriteNextVersion");
                return false;
            }

            var versionFile = _args[0];
            var versionCounterFile = _args[1];
            var outFile = _args[2];

            var success = Commands.TryWriteNextVersionFromFile(versionFile, versionCounterFile, outFile);
            return success;
        }

        private static bool TryWriteNextVersion(string[] _args)
        {
            Debug.Assert(_args != null);

            if (_args.Length < 3)
            {
                Console.WriteLine("Insufficient args in TryWriteNextVersion");
                return false;
            }

            var success = false;

            var versionRaw = _args[0];
            var versionsFile = _args[1];
            var outFile = _args[2];

            if (Commands.TryWriteNextVersion(versionRaw, versionsFile, outFile))
            {
                success = true;
            }

            return success;
        }


        private static bool TryWriteNextVersionFromFile2(string[] _args)
        {
            Debug.Assert(_args != null);

            if (_args.Length < 3)
            {
                Console.WriteLine("Insufficient args in TryWriteNextVersion");
                return false;
            }

            var versionFile = _args[0];
            int buildNumber;
            if (!int.TryParse(_args[1], out buildNumber))
            {
                Console.WriteLine("Build Number (Arg 2) not an int");
                return false;
            }
            var outFile = _args[2];

            var success = Commands.TryWriteNextVersionFromFile(versionFile, buildNumber, outFile);
            return success;
        }
    }
}