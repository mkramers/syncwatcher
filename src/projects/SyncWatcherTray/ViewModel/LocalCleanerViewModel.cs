﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using Common.Mvvm;
using FilebotApi;
using Common.Logging;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;
using PlexTools;

namespace SyncWatcherTray.ViewModel
{
    public class LocalCleanerViewModel : ViewModelBase
    {
        public DirectoryViewModel DirectoryViewModel { get; }
        public Filebot Filebot { get; }
        private string OutputDirectory { get; }
        public FileWatcher FileWatcher { get; }
        public bool IsBusy { get; private set; }

        public ICommand OrganizeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        OnStarted();

                        await Organize();
                        await ScanPlex();

                        OnStopped();
                    },
                    CanExecuteFunc = () => Filebot != null && !IsBusy
                };
            }
        }

        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopped;

        public LocalCleanerViewModel(SourceDestinationPaths _paths, Filebot _filebot)
        {
            Debug.Assert(_paths != null);

            string inputDir = _paths.SourcePath;
            string outputDir = _paths.DestinationPath;

            Debug.Assert(!string.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(outputDir));
            Debug.Assert(_filebot != null);

            DirectoryViewModel = new DirectoryViewModel(inputDir, "Complete");
            OutputDirectory = outputDir;

            Filebot = _filebot;

            FileWatcher = new FileWatcher(inputDir);
            FileWatcher.WatchEvent += FileWatcher_WatchEvent;
            FileWatcher.Start();
        }

        private async Task Organize()
        {
            string input = DirectoryViewModel.Name;
            string output = OutputDirectory;

            Filebot fileBot = Filebot;

            await Task.Run(() => fileBot.Organize(input, output));
        }

        private static async Task ScanPlex()
        {
            uint[] sections = { 4, 5, 6 };

            Log.Write(LogLevel.Info, "Starting Plex scan...");

            try
            {
                await Task.Factory.StartNew(() => PlexScanner.ScanSections(sections));

                Log.Write(LogLevel.Info, "Plex scan completed.");
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Info, $"Error scanning plex: {e.Message}");
            }
        }

        private void RefreshCompletedDirectory()
        {
            ICommand refreshCommand = DirectoryViewModel.RefreshCommand;
            if (refreshCommand.CanExecute(null))
                refreshCommand.Execute(null);
        }

        private void OnStarted()
        {
            IsBusy = true;

            DirectoryViewModel.IsBusy = true;

            Started?.Invoke(this, EventArgs.Empty);
        }

        private void OnStopped()
        {
            IsBusy = false;

            DirectoryViewModel.IsBusy = false;
            
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void FileWatcher_WatchEvent(object _sender, FileSystemEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);
        }
    }
}