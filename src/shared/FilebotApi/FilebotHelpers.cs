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
        public static Filebot InitializeFilebot(string _appDataDirectory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));

            if (!Directory.Exists(_appDataDirectory))
            {
                Directory.CreateDirectory(_appDataDirectory);
            }

            string settingsPath = Path.Combine(_appDataDirectory, "settings.xml");
            FilebotSettings settings = LoadSettingsSafely(settingsPath);

            string logPath = Path.Combine(_appDataDirectory, "amclog.txt");
            FilebotLog log = new FilebotLog(logPath);

            Filebot filebot = new Filebot(settings, log);
            return filebot;
        }

        private static FilebotSettings LoadSettingsSafely(string _settingsPath)
        {
            FilebotSettings settings;

            try
            {
                settings = FilebotSettings.Load(_settingsPath);
            }
            catch (IOException)
            {
                //if we fail to load, try saving the default and retrying
                try
                {
                    FilebotSettings.Save(FilebotSettings.Default, _settingsPath);

                    settings = FilebotSettings.Load(_settingsPath);
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