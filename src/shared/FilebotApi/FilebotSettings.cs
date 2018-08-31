﻿using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Common;
using Newtonsoft.Json;

namespace FilebotApi
{
    public class FilebotSettings
    {
        public static FilebotSettings Load(string _fileName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_fileName));

            FilebotSettings settings;

            try
            {
                settings = Utilities.XmlDeserializeObject<FilebotSettings>(_fileName);
                settings.SettingsFilePath = _fileName;
            }
            catch (Exception e)
            {
                string message = $"Filebot settings failed to load from {Path.GetFullPath(_fileName)}\n\n{e}";
                throw new FileLoadException(message, e);
            }

            return settings;
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
                string message = $"Filebot settings failed to save to {Path.GetFullPath(_fileName)}";
                throw new IOException(message, e);
            }
        }

        public void Save()
        {
            Save(this, SettingsFilePath);
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
                    FilebotBinaries = @"C:\Program Files\FileBot",
                    FilebotScriptsDirectory = @"C:\Program Files\FileBot\scripts"
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
        public string FilebotScriptsDirectory { get; set; }

        [JsonIgnore]
        public string SettingsFilePath { get; set; }
        public string FilebotExe => Path.Combine(FilebotBinaries, "filebot.exe");
        public string FilebotJar => Path.Combine(FilebotBinaries, "FileBot.jar");
    }
}