using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Common.Logging;
using FilebotApi.Result;
using GalaSoft.MvvmLight;

namespace FilebotApi
{
    public partial class Filebot : ViewModelBase
    {
        private bool m_isBusy;

        public FilebotSettings Settings { get; }
        public FilebotRecords Records { get; }
        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                if (m_isBusy != value)
                {
                    m_isBusy = value;
                    BusyChanged?.Invoke(this, EventArgs.Empty);
                    RaisePropertyChanged();
                }
            }
        }

        public event EventHandler Started;
        public event EventHandler<FileBotOrganizeEventArgs> Stopped;
        public event EventHandler<EventArgs> BusyChanged;

        public Filebot(FilebotSettings _settings, FilebotRecords _records)
        {
            Debug.Assert(_settings != null);
            Debug.Assert(_records != null);

            Settings = _settings;

            Records = _records;
        }
        
        public void Organize(string _inputDir, string _outputDir)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));

            string message = $"Starting organize...\nSource: {_inputDir}\nTarget: {_outputDir}";
            Log.Write(LogLevel.Info, message);

            OrganizeResult organizeResult;

            IsBusy = true;

            Started?.Invoke(this, EventArgs.Empty);

            try
            {
                FilebotSettings settings = Settings;
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

                organizeResult = OrganizeResult.SUCCESS;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error running filebot: {e}");
                organizeResult = OrganizeResult.FAIL;
            }

            //reparse records
            Records.Reload();

            IsBusy = false;

            FileBotOrganizeEventArgs args = new FileBotOrganizeEventArgs(organizeResult);
            Stopped?.Invoke(this, args);

            Log.Write(LogLevel.Info, "Completed");
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

            Log.Write(LogLevel.Info, message);
        }

        private static string GetArguments(string _inputPath, string _outputPath, FilebotSettings _settings)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputPath));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputPath));
            Debug.Assert(_settings != null);

            StringBuilder argument = new StringBuilder();

            const string scriptPath = "filebot\\scripts\\amc.groovy";
            argument.Append($"-script {scriptPath}"); //todo fix path

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

        private static ProcessStartInfo GetFileBotProcessInfo(string _inputDir, string _outputDir, FilebotSettings _settings)
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