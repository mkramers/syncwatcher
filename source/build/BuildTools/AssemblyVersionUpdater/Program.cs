using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyVersionUpdater
{
    public class Program
    {
        private static void Main(string[] _args)
        {
            var errorCode = CheckInputArgs(_args);

            switch (errorCode)
            {
                case ErrorCode.NONE:
                    Run(_args);
                    break;
                case ErrorCode.INVALID_ARGS:
                case ErrorCode.FILE_DOESNT_EXISTS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ErrorCode CheckInputArgs(string[] _args)
        {
            var errorCode = ErrorCode.NONE;

            if (_args != null)
            {
                if (_args.Length != 2)
                {
                    errorCode = ErrorCode.INVALID_ARGS;
                }
            }

            return errorCode;
        }

        private static void DisplayError(ErrorCode _code)
        {
            switch (_code)
            {
                case ErrorCode.NONE:
                    break;
                case ErrorCode.INVALID_ARGS:
                    break;
                case ErrorCode.FILE_DOESNT_EXISTS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_code), _code, null);
            }
        }

        private static ErrorCode Run(string[] _args)
        {
            var error = ErrorCode.NONE;

            Version version;
            if (TryGetVersion(_args[1], out version))
            {
                var path = _args[0];
                if (!File.Exists(path))
                {
                    error = ErrorCode.FILE_DOESNT_EXISTS;
                }
                else
                {
                    UpdateVersion(path, version);
                }
            }

            return error;
        }


        //helper

        public static void UpdateVersion(string _assemblyFilePath, Version _version)
        {
            UpdateVersion(_assemblyFilePath, _version.ToString());
        }

        public static void UpdateVersion(string _assemblyFilePath, string _version)
        {
            Debug.Assert(_assemblyFilePath != null, "_assemblyFilePath != null");
            Debug.Assert(_version != null, "_version != null");
            Debug.Assert(File.Exists(_assemblyFilePath));

            var keys = new[]
            {
                "[assembly: AssemblyVersion(\"",
                "[assembly: AssemblyFileVersion(\"",
            }.ToList();

            const string endLine = "\")]";

            var newFile = new StringBuilder();

            var lines = File.ReadAllLines(_assemblyFilePath);
            foreach (var line in lines)
            {
                string newLine;
                var foundKeys = keys.FindAll(_key => line.StartsWith(_key));
                Debug.Assert(foundKeys.Count <= 1);

                var key = foundKeys.FirstOrDefault();
                if (key != null)
                {
                    var version = _version;
                    newLine = $"{key}{version}{endLine}";
                }
                else
                {
                    newLine = line;
                }

                newFile.AppendLine(newLine);
            }

            var output = newFile.ToString();

            File.WriteAllText(_assemblyFilePath, output);
        }

        public static bool TryGetVersion(string _input, out Version _version)
        {
            _version = null;

            var valid = false;

            if (!string.IsNullOrWhiteSpace(_input))
            {
                var split = _input.Split('.');
                if (split.Length > 2)
                {
                    if (split.Length > 4)
                    {
                        Console.WriteLine(@"Warning: Versions must be exactly 4 digits");
                    }

                    int major = 0;
                    int minor = 0;
                    int revision = 0;
                    int changelist = 0;

                    var charactersValid = true;
                    for (var i = 0; i < split.Length; i++)
                    {
                        int value;
                        if (int.TryParse(split[i], out value))
                        {
                            switch (i)
                            {
                                case 0:
                                    major = value;
                                    break;
                                case 1:
                                    minor = value;
                                    break;
                                case 2:
                                    revision = value;
                                    break;
                                case 3:
                                    changelist = value;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            charactersValid = false;
                            Console.WriteLine($@"Version character digit '{split[i]}' is a non integer!");
                            break;
                        }
                    }

                    if (charactersValid)
                    {
                        _version = new Version(major, minor, revision, changelist);
                    }
                }
                else
                {
                    Console.WriteLine($@"Version '{_input}' must contain at least 2 identifiers (i.e. 1.0)");
                }

                valid = true;
            }

            return valid;
        }

        private enum ErrorCode
        {
            NONE,
            INVALID_ARGS,
            FILE_DOESNT_EXISTS
        }
    }
}
