using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;
using System.Collections.Generic;
using Common.IO;

namespace FilebotApi
{
    public static class FilebotHelpers
    {
        public static bool TryCreateFilebot(string _appDataDirectory, out Filebot _filebot)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));

            _filebot = null;

            bool success = false;

            string settingsPath = Path.Combine(_appDataDirectory, "settings.xml");
            string recordsPath = Path.Combine(_appDataDirectory, "amclog.txt");

            if (TryCreateFilebot(settingsPath, recordsPath, out _filebot))
            {
                success = true;
            }

            return success;
        }

        public static bool TryCreateFilebot(string _settingsPath, string _recordsPath, out Filebot _filebot)
        {
            _filebot = null;

            bool success = false;

            if (!File.Exists(_settingsPath))
                FilebotSettings.CreateDefaultSettingsFile(_settingsPath);

            FilebotRecords records = new FilebotRecords(_recordsPath);
            records.Reload();

            if (FilebotSettings.TryLoad(_settingsPath, out FilebotSettings settings))
            {
                _filebot = new Filebot(settings, records);
                success = true;
            }

            return success;
        }
    }
}