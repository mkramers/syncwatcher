using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace P4Commands
{
    public static class Commands
    {
        //create a new changelist number
        public static bool TryCreateChangelist(string _description, string _tempFile, out int _changelist)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_description));
            Debug.Assert(!string.IsNullOrWhiteSpace(_tempFile));

            _changelist = 0;

            var success = false;

            //build file
            var text = GetChangeText(_description);
            File.WriteAllText(_tempFile, text);

            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = $"/C p4 change -i < \"{_tempFile}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.StartInfo = startInfo;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();

                //Console.WriteLine($"p4 > {line}");

                if (!string.IsNullOrWhiteSpace(line))
                {
                    var split = line.Split(' ');
                    if (split.Length >= 3)
                    {
                        if (split[0] == "Change" && split[2] == "created.")
                        {
                            success = int.TryParse(split[1], out _changelist);
                            break;
                        }
                    }
                }
            }

            process.WaitForExit();

            if (File.Exists(_tempFile))
            {
                try
                {
                    File.Delete(_tempFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error deleting temp file: {e.Message}.");
                }
            }
            
            return success;
        }

        public static bool TryWriteChangelistToFile(string _description, string _outputFile, out int _changelist)
        {
            var success = false;

            var tempFile = Path.GetTempFileName();
            if (TryCreateChangelist(_description, tempFile, out _changelist))
            {
                try
                {
                    File.WriteAllText(_outputFile, _changelist.ToString());
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error writing changelist: {e.Message}");
                }
            }

            return success;
        }

        //returns a version number containing the next rev number given a raw majot.minor version (i.e. 1.0)
        public static bool GetNextRevision(string _versionRaw, string _versionFile, out Version _version)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionRaw));
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionFile));

            var success = false;

            if (Version.TryParse(_versionRaw, out _version))
            {
                var revision = GetNextRevision(_version, _versionFile);
                if (revision != null)
                {
                    _version.Revision = revision;

                    success = true;
                }
            }

            return success;
        }

        //returns a version number containing the next rev number given a file which contains a raw major.minor version (i.e. 1.0)
        public static bool TryWriteNextVersionFromFile(string _versionFile, string _versionCounterFile, string _outFile)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionFile));
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionCounterFile));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outFile));

            var success = false;

            if (File.Exists(_versionFile))
            {
                var versionFileLines = File.ReadAllLines(_versionFile);
                if (versionFileLines.Length > 0)
                {
                    var verisonRaw = versionFileLines.First();

                    success = TryWriteNextVersion(verisonRaw, _versionCounterFile, _outFile);
                }

            }

            return success;
        }

        //returns a version number containing the next rev number given a file which contains a raw major.minor version (i.e. 1.0)
        public static bool TryWriteNextVersionFromFile(string _versionFile, int _buildNumber, string _outFile)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionFile));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outFile));

            var success = false;

            if (File.Exists(_versionFile))
            {
                var versionFileLines = File.ReadAllLines(_versionFile);
                if (versionFileLines.Length > 0)
                {
                    var verisonRaw = versionFileLines.First();

                    success = TryWriteNextVersion(verisonRaw, _buildNumber, _outFile);
                }
            }

            return success;
        }

        //returns a version number containing the next rev number given a raw major.minor version (i.e. 1.0)
        public static bool TryWriteNextVersion(string _versionRaw, string _versionsFile, string _outFile)
        {
            var success = false;

            Version version;
            if (TryGetNextVersion(_versionRaw, _versionsFile, out version))
            {
                File.WriteAllText(_outFile, version.ToString());

                success = true;
            }

            return success;
        }

        //returns a version number containing the next rev number given a raw major.minor version (i.e. 1.0)
        public static bool TryWriteNextVersion(string _versionRaw, int _buildNumber, string _outFile)
        {
            var success = false;

            Version version;
            if (TryGetNextVersion(_versionRaw, _buildNumber, out version))
            {
                File.WriteAllText(_outFile, version.ToString());

                success = true;
            }

            return success;
        }


        //returns a version number containing the next rev number given a file which contains a raw major.minor version (i.e. 1.0)
        private static bool TryGetNextVersion(string _versionRaw, string _versionsFile, out Version _version)
        {
            var success = false;

            if (Version.TryParse(_versionRaw, out _version))
            {
                var revision = GetNextRevision(_version, _versionsFile);
                if (revision != null)
                {
                    //get the build #
                    _version.Revision = revision;

                    //get the cl
                    var cl = GetChangelist();
                    if (cl != null)
                    {
                        _version.Changelist = cl;
                        Console.WriteLine($"Got version: {_version}");
                        success = true;
                    }
                }
            }
            return success;
        }

        //returns a version number containing the next rev number given a file which contains a raw major.minor version (i.e. 1.0)
        private static bool TryGetNextVersion(string _versionRaw, int _buildNumber, out Version _version)
        {
            var success = false;

            if (Version.TryParse(_versionRaw, out _version))
            {
                //get the build #
                _version.Revision = _buildNumber;

                //get the cl
                var cl = GetChangelist();
                if (cl != null)
                {
                    _version.Changelist = cl;
                    Console.WriteLine($"Got version: {_version}");
                    success = true;
                }
            }
            return success;
        }


        //helpers

        private static int? GetNextRevision(Version _version, string _versionFile)
        {
            Debug.Assert(_version != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionFile));

            int? nextRevision = 0;

            var newVersion = new Version(_version.Major, _version.Minor, 0);

            var versions = new List<Version>();

            if (File.Exists(_versionFile))
            {
                var existingVersions = Utilities.XmlDeserializeObject<List<Version>>(_versionFile);
                if (existingVersions != null)
                {
                    var existingVersion =
                        existingVersions.Find(
                            _existing => _existing.Major == _version.Major && _existing.Minor == _version.Minor);

                    if (existingVersion != null)
                    {
                        if (existingVersion.Revision != null)
                        {
                            existingVersion.Revision += 1;
                        }
                        else
                        {
                            existingVersion.Revision = 0;
                        }

                        nextRevision = existingVersion.Revision;
                    }
                    else
                    {
                        existingVersions.Add(newVersion);
                    }

                    versions.AddRange(existingVersions);
                }
            }
            else
            {
                versions.Add(newVersion);
            }

            Utilities.XmlSerializeObject(versions, _versionFile);

            return nextRevision;
        }

        public static int? GetChangelist()
        {
            int? changeListOut = null;

            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = "/C p4 changes -s submitted -m 1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.StartInfo = startInfo;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();

                Console.WriteLine($"p4 > {line}");

                if (!string.IsNullOrWhiteSpace(line))
                {
                    var split = line.Split(' ');
                    if (split.Length >= 3)
                    {
                        if (split[0] == "Change" && split[2] == "on")
                        {
                            int changeList;
                            if (int.TryParse(split[1], out changeList))
                            {
                                changeListOut = changeList;
                                break;
                            }
                        }
                    }
                }
            }

            return changeListOut;
        }

        private static string GetChangeText(string _description)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_description));

            var output = new StringBuilder();
            output.AppendLine($"Change: new\nStatus: new\nDescription: {_description}");

            return output.ToString();
        }
    }
}