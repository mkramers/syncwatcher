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
        public static Filebot CreateFilebot(string _appDataDirectory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));

            if (!Directory.Exists(_appDataDirectory))
            {
                Directory.CreateDirectory(_appDataDirectory);
            }

            string settingsPath = Path.Combine(_appDataDirectory, "settings.xml");
            string recordsPath = Path.Combine(_appDataDirectory, "amclog.txt");

            FilebotRecords records = new FilebotRecords(recordsPath);

            FilebotSettings settings = LoadSettingsSafely(settingsPath);
            Debug.Assert(settings != null);

            Filebot filebot = new Filebot(settings, records);
            return filebot;
        }

        private static FilebotSettings LoadSettingsSafely(string _settingsPath)
        {
            FilebotSettings settings = null;

            try
            {
                settings = FilebotSettings.Load(_settingsPath);
            }
            catch (FileLoadException)
            {
                //if we fail to load, try saving the default and retrying
                try
                {
                    FilebotSettings.Save(FilebotSettings.Default, _settingsPath);
                }
                catch (IOException)
                {
                    //if all else failes, load the default settings to memory
                    settings = FilebotSettings.Default;
                }
            }
            return settings;
        }
    }
}