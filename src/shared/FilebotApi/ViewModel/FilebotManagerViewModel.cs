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

namespace FilebotApi.ViewModel
{
    public class FilebotManagerViewModel : ViewModelBase
    {
        public Filebot Filebot { get; }
        
        public event EventHandler FilebotStarted
        {
            add => Filebot.Started += value;
            remove => Filebot.Started -= value;
        }
        public event EventHandler<FileBotOrganizeEventArgs> FilebotStopped
        {
            add => Filebot.Stopped += value;
            remove => Filebot.Stopped -= value;
        }

        public FilebotManagerViewModel(SourceDestinationPaths _paths, Filebot _filebot)
        {
            Debug.Assert(_paths != null);
            Debug.Assert(_filebot != null);

            Filebot = _filebot;
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