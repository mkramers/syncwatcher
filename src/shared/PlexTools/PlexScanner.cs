using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common.Logging;

namespace PlexTools
{
    public class PlexScanner
    {
        private const string PLEX_EXE_DIRECTORY = @"C:\Program Files (x86)\Plex\Plex Media Server";
        private static readonly string PlexScannerExe = Path.Combine(PLEX_EXE_DIRECTORY, "Plex Media Scanner.exe");

        public static void ScanSections(IEnumerable<uint> _sections)
        {
            Debug.Assert(_sections != null);

            foreach (uint section in _sections)
            {
                ScanSection(section);
            }
        }

        public static void ScanSection(uint _section)
        {
            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = PLEX_EXE_DIRECTORY,
                FileName = PlexScannerExe,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                Arguments = $"--scan --section {_section}"
            };

            using (Process exeProcess = Process.Start(startInfo))
            {
                Debug.Assert(exeProcess != null);

                exeProcess.WaitForExit();
            }
        }
    }
}