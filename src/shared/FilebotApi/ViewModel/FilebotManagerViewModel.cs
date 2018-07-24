using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;
using System.Collections.Generic;

namespace FilebotApi.ViewModel
{
    public class FilebotManagerViewModel : ViewModelBase
    {
        public Filebot Filebot { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }
        public IEnumerable<DirectoryViewModel> Directories { get; }

        public FilebotManagerViewModel(IEnumerable<DirectoryViewModel> _directories)
        {
            Debug.Assert(_directories != null);
            
            Directories = _directories;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string directory = Path.Combine(appData, "SyncWatcher");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string settingsPath = Path.Combine(directory, "settings.xml");
            string recordsPath = Path.Combine(directory, "amclog.txt");

            if (Filebot.TryCreate(settingsPath, recordsPath, out Filebot filebot))
            {
                filebot.Stopped += Filebot_Completed;

                Filebot = filebot;
            }

            const string input = @"D:\Unsorted\completed";
            const string outputDir = @"F:\Videos";

            CompletedDirectory = new LocalCleanerViewModel(input, outputDir, filebot);
        }

        private void Filebot_Completed(object _sender, Filebot.FileBotOrganizeEventArgs _e)
        {
            RefreshCompletedDirectory();
        }

        private void RefreshCompletedDirectory()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ICommand refreshCommand = CompletedDirectory.DirectoryViewModel.RefreshCommand;
                if (refreshCommand.CanExecute(null))
                    refreshCommand.Execute(null);
            });
        }

        public event EventHandler FilebotStarted
        {
            add => Filebot.Started += value;
            remove => Filebot.Started -= value;
        }
        public event EventHandler<Filebot.FileBotOrganizeEventArgs> FilebotCompleted
        {
            add => Filebot.Stopped += value;
            remove => Filebot.Stopped -= value;
        }
    }
}