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
    public class Filebot : ViewModelBase
    {
        private Filebot(FilebotSettings _settings, string _recordsPath)
        {
            Debug.Assert(_settings != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_recordsPath));

            Settings = _settings;
            Records = new FilebotRecords();
            Records.RequestRefresh += Records_RequestRefresh;

            RecordsFile = _recordsPath;

            LoadRecords(_recordsPath, Records);
        }

        public static bool TryCreate(string _settingsPath, string _recordsPath, out Filebot _filebot)
        {
            _filebot = null;

            var success = false;

            if (!File.Exists(_settingsPath))
            {
                FilebotSettings.CreateDefaultSettingsFile(_settingsPath);
            }

            if (TryLoad(_settingsPath, _recordsPath, out _filebot))
            {
                success = true;
            }

            return success;
        }

        private static bool TryLoad(string _settingsPath, string _recordsPath, out Filebot _filebot)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_settingsPath));
            Debug.Assert(_recordsPath != null);

            _filebot = null;

            var success = false;

            if (FilebotSettings.TryLoad(_settingsPath, out var settings))
            {
                _filebot = new Filebot(settings, _recordsPath)
                {
                    SettingsFile = _settingsPath,
                };

                success = true;
            }

            return success;
        }

        private void Records_RequestRefresh(object _sender, EventArgs _e)
        {
            var path = RecordsFile;
            var records = Records;
            LoadRecords(path, records);
        }

        private static void LoadRecords(string _recordsPath, FilebotRecords _records)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_recordsPath));
            Debug.Assert(_records != null);

            var lines = File.ReadAllLines(_recordsPath);
            var renameResults = new List<RenameResult>();
            var skipResults = new List<SkipResult>();
            foreach (var line in lines)
            {
                if (FileBotLogParser.TryParse(line, out var result))
                {
                    switch (result)
                    {
                        case RenameResult renameResult:
                            renameResults.Add(renameResult);
                            break;
                        case SkipResult skipResult:
                            skipResults.Add(skipResult);
                            break;
                    }
                }
            }

            _records.Update(renameResults, skipResults);
        }

        public void Organize(string _inputDir, string _outputDir)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));

            var message = $"Starting organize...\nSource: {_inputDir}\nTarget: {_outputDir}";
            Log.Write(LogLevel.Info, message);

            OrganizeResult organizeResult;

            IsBusy = true;

            Started?.Invoke(this, EventArgs.Empty);

            try
            {
                var settings = Settings;
                var startInfo = GetFileBotProcessInfo(_inputDir, _outputDir, settings);

                using (var exeProcess = Process.Start(startInfo))
                {
                    Debug.Assert(exeProcess != null);

                    string line;
                    while ((line = exeProcess.StandardOutput.ReadLine()) != null)
                    {
                        if (FileBotLogParser.TryParse(line, out var result))
                        {
                            LogResult(result);
                        }
                    }

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
            var recordsPath = RecordsFile;
            var records = Records;
            LoadRecords(recordsPath, records);

            IsBusy = false;

            var args = new FileBotOrganizeEventArgs(organizeResult);
            Stopped?.Invoke(this, args);

            Log.Write(LogLevel.Info, "Completed");
        }

        private void LogResult(FileBotResult _result)
        {
            Debug.Assert(_result != null);

            var rename = _result as RenameResult;
            var result = _result;

            string message;
            if (rename != null)
            {
                var dest = rename.ProposedFile;
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

            var argument = new StringBuilder();

            const string scriptPath = "filebot\\scripts\\amc.groovy";
            argument.Append($"-script {scriptPath}"); //todo fix path

            Debug.Assert(!string.IsNullOrWhiteSpace(_outputPath));
            Debug.Assert(Directory.Exists(_outputPath));
            argument.AppendFormat(" --output \"{0}\"", _outputPath);

            Debug.Assert(!string.IsNullOrWhiteSpace(_inputPath));
            Debug.Assert(Directory.Exists(_inputPath));
            var isNonStrict = _settings.IsNonStrict ? " -non-strict" : "";
            argument.AppendFormat(" --action {0}{1} \"{2}\"", _settings.Action.ToString().ToLower(), isNonStrict,
                _inputPath);

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

        private static ProcessStartInfo GetFileBotProcessInfo(string _inputDir, string _outputDir,
            FilebotSettings _settings)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));
            Debug.Assert(_settings != null);

            var fileBotExecutable = _settings.FilebotExe;
            var fileBotJar = _settings.FilebotJar;
            if (!File.Exists(fileBotExecutable) || !File.Exists(fileBotJar))
            {
                var message =
                    $"Filebot executable <{Path.GetFullPath(fileBotExecutable)}> or jar <{fileBotJar}> not found!";
                throw new FileNotFoundException(message);
            }

            var fileBotArgument = GetArguments(_inputDir, _outputDir, _settings);

            var startInfo = new ProcessStartInfo
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
        
        public event EventHandler Started;
        public event EventHandler<FileBotOrganizeEventArgs> Stopped;
        public event EventHandler<EventArgs> BusyChanged;

        public FilebotSettings Settings { get; }
        public string SettingsFile { get; private set; }
        public FilebotRecords Records { get; }
        public string RecordsFile { get; }

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

        private bool m_isBusy;

        public enum ActionType
        {
            MOVE,
            COPY,
            TEST
        }

        public enum OrganizeResult
        {
            SUCCESS,
            FAIL
        }


        public class FileBotOrganizeEventArgs : EventArgs
        {
            public FileBotOrganizeEventArgs(OrganizeResult _result)
            {
                Result = _result;
            }

            public OrganizeResult Result { get; }
        }
    }
}