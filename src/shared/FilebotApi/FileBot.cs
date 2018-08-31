using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Common.Logging;
using FilebotApi.Properties;
using FilebotApi.Result;
using GalaSoft.MvvmLight;

namespace FilebotApi
{
    public partial class Filebot : ViewModelBase
    {
        private readonly Settings m_settings;

        public FilebotLog Log { get; }
        
        public Filebot(Settings _settings, FilebotLog _log)
        {
            Debug.Assert(_settings != null);
            Debug.Assert(_log != null);

            m_settings = _settings;
            Log = _log;
        }
        
        public void Organize(string _inputDir, string _outputDir)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));

            string message = $"Starting organize...\nSource: {_inputDir}\nTarget: {_outputDir}";
            Common.Logging.Log.Write(LogLevel.Info, message);

            try
            {
                Settings settings = m_settings;
                ProcessStartInfo startInfo = GetFileBotProcessInfo(_inputDir, _outputDir, settings);

                using (Process exeProcess = Process.Start(startInfo))
                {
                    Debug.Assert(exeProcess != null);

                    string line;
                    while ((line = exeProcess.StandardOutput.ReadLine()) != null)
                        if (FileBotLogParser.TryParse(line, out FileBotResult result))
                            LogResult(result);

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error running filebot: {e}");
            }

            //reparse records
            Log.Reload();

            Common.Logging.Log.Write(LogLevel.Info, "Completed");
        }

        private void LogResult(FileBotResult _result)
        {
            Debug.Assert(_result != null);

            RenameResult rename = _result as RenameResult;
            FileBotResult result = _result;

            string message;
            if (rename != null)
            {
                string dest = rename.ProposedFile;
                message = $"[Rename]: {Path.GetFileName(dest)}?";
            }
            else
            {
                message = $"[Log]: {result.RawLine}";
            }

            Common.Logging.Log.Write(LogLevel.Info, message);
        }

        private static string GetArguments(string _inputPath, string _outputPath, Settings _settings)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputPath));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputPath));
            Debug.Assert(_settings != null);

            StringBuilder argument = new StringBuilder();

            string scriptPath = Path.Combine(_settings.FilebotScriptsDirectory, "amc.groovy");
            argument.Append($"-script \"{scriptPath}\""); //todo fix path

            Debug.Assert(!string.IsNullOrWhiteSpace(_outputPath));
            Debug.Assert(Directory.Exists(_outputPath));
            argument.AppendFormat(" --output \"{0}\"", _outputPath);

            Debug.Assert(!string.IsNullOrWhiteSpace(_inputPath));
            Debug.Assert(Directory.Exists(_inputPath));
            string isNonStrict = _settings.IsNonStrict ? " -non-strict" : "";
            argument.AppendFormat(" --action {0}{1} \"{2}\"", _settings.Action.ToString().ToLower(), isNonStrict, _inputPath);

            if (_settings.Clean)
                argument.AppendFormat(" --def clean=y");

            if (_settings.DeleteAfterExtract)
                argument.AppendFormat(" --def deleteAfterExtract=y");

            argument.AppendFormat(" --def minFileSize=30000000");

            if (_settings.UseLogFile)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(_settings.LogFilePath));
                argument.AppendFormat(" --log-file \"{0}\"", _settings.LogFilePath);
            }

            if (_settings.UseExcludeList)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(_settings.ExcludeListPath));
                argument.AppendFormat(" --def excludeList=\"{0}\"", _settings.ExcludeListPath);
            }

            return argument.ToString();
        }

        private static ProcessStartInfo GetFileBotProcessInfo(string _inputDir, string _outputDir, Settings _settings)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));
            Debug.Assert(_settings != null);

            string fileBotExecutable = _settings.FilebotExe;
            string fileBotJar = _settings.FilebotJar;
            if (!File.Exists(fileBotExecutable) || !File.Exists(fileBotJar))
            {
                string message = $"Filebot executable <{Path.GetFullPath(fileBotExecutable)}> or jar <{fileBotJar}> not found!";
                throw new FileNotFoundException(message);
            }

            string fileBotArgument = GetArguments(_inputDir, _outputDir, _settings);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                FileName = fileBotExecutable,
                Arguments = fileBotArgument
            };
            return startInfo;
        }
    }
}