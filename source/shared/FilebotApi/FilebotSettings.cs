using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Common;

namespace FilebotApi
{
    public class FilebotSettings
    {
        public static bool TryLoad(string _fileName, out FilebotSettings _settings)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_fileName));

            _settings = null;

            try
            {
                _settings = Utilities.XmlDeserializeObject<FilebotSettings>(_fileName);
            }
            catch (Exception e)
            {
                var message = $"Filebot settings failed to load from {Path.GetFullPath(_fileName)}\n\n{e}";
                Console.WriteLine(message);
            }

            return _settings != null;
        }

        public static void Save(FilebotSettings _settings, string _fileName)
        {
            Debug.Assert(_settings != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_fileName));

            try
            {
                Utilities.XmlSerializeObject(_settings, _fileName);
            }
            catch (Exception e)
            {
                var message = $"Filebot settings failed to save to {Path.GetFullPath(_fileName)}";
                throw new XmlException(message, e);
            }
        }

        public static void CreateDefaultSettingsFile(string _fileName)
        {
            Debug.Assert(_fileName != null);

            Save(Default, _fileName);
        }

        public static FilebotSettings Default
        {
            get
            {
                var settings = new FilebotSettings
                {
                    Action = Filebot.ActionType.MOVE,
                    Clean = true,
                    DeleteAfterExtract = true,
                    UseExcludeList = true,
                    ExcludeListPath = Path.GetFullPath("amc.ignore.txt"),
                    UseLogFile = true,
                    IsNonStrict = true,
                    LogFilePath = Path.GetFullPath("amc.log.txt"),
                    FilebotBinaries = @"D:\Unsorted\filebot"
                };
                return settings;
            }
        }

        public Filebot.ActionType Action { get; set; }
        public bool Clean { get; set; }
        public bool DeleteAfterExtract { get; set; }
        public bool UseExcludeList { get; set; }
        public string ExcludeListPath { get; set; }
        public bool UseLogFile { get; set; }
        public string LogFilePath { get; set; }
        public bool IsNonStrict { get; set; }
        public string FilebotBinaries { get; set; }
        public string FilebotExe => Path.Combine(FilebotBinaries, "filebot.exe");
        public string FilebotJar => Path.Combine(FilebotBinaries, "FileBot.jar");
    }
}