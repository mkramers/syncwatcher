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
        public LocalCleanerViewModel CompletedDirectory { get; }
        public IEnumerable<DirectoryViewModel> Directories { get; }

        public FilebotManagerViewModel(string _appDataDirectory, SourceDestinationPaths _paths, IEnumerable<DirectoryViewModel> _directories)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));
            Debug.Assert(_paths != null);
            Debug.Assert(_directories != null);

            Directories = _directories;

            string settingsPath = Path.Combine(_appDataDirectory, "settings.xml");
            string recordsPath = Path.Combine(_appDataDirectory, "amclog.txt");

            if (Filebot.TryCreate(settingsPath, recordsPath, out Filebot filebot))
            {
                filebot.Stopped += Filebot_Completed;

                Filebot = filebot;
            }

            CompletedDirectory = new LocalCleanerViewModel(_paths, filebot);
        }

        private void Filebot_Completed(object _sender, FileBotOrganizeEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);
        }

        private void RefreshCompletedDirectory()
        {
            ICommand refreshCommand = CompletedDirectory.DirectoryViewModel.RefreshCommand;
            if (refreshCommand.CanExecute(null))
                refreshCommand.Execute(null);
        }

        public event EventHandler FilebotStarted
        {
            add => Filebot.Started += value;
            remove => Filebot.Started -= value;
        }
        public event EventHandler<FileBotOrganizeEventArgs> FilebotCompleted
        {
            add => Filebot.Stopped += value;
            remove => Filebot.Stopped -= value;
        }
    }
}