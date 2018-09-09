using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.Framework.EventHelpers;
using Common.IO;
using Common.Mvvm;
using Common.SFTP;
using FilebotApi;
using GalaSoft.MvvmLight;
using MVVM.Popups;
using MVVM.ViewModel;
using SyncWatcherTray.Properties;
using WinScpApi;
using WinScpApi.ViewModel;
using Application = System.Windows.Application;

namespace SyncWatcherTray.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public FtpManagerViewModel FtpManagerViewModel { get; }
        public TaskbarIconViewModel TaskBarIcon { get; }
        public IEnumerable<DirectoryViewModel> Directories { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }

        public MainViewModel()
        {
            TaskBarIcon = new TaskbarIconViewModel();

            Settings settings = Settings.Default;

            string input = settings.CompletedDirectory;
            string outputDir = settings.MediaRootDirectory;
            string seriesDir = settings.SeriesDirectory;
            string moviesDir = settings.MovieDirectory;

            SourceDestinationPaths paths = new SourceDestinationPaths(input, outputDir);

            ValidateSettings(paths);

            FtpManagerViewModel = InitializeFtpManager(paths.SourcePath);

            string appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDataDirectory = Path.Combine(appDataRoot, "SyncWatcherTray");

            //create app data dir
            Directory.CreateDirectory(appDataDirectory);

            CompletedDirectory = new LocalCleanerViewModel(paths, appDataDirectory);
            CompletedDirectory.Started += Operation_Started;
            CompletedDirectory.Stopped += OperationStopped;

            Directories = new[]
            {
                new DirectoryViewModel(seriesDir, "TV"),
                new DirectoryViewModel(moviesDir, "MOVIES")
            };

            RunPostOperations();
        }

        private FtpManagerViewModel InitializeFtpManager(string _input)
        {
            SessionConfig ftpSessionConfig = SessionConfig.Default;

            FtpManager manager = new FtpManager(ftpSessionConfig, new List<string>
            {
                _input
            });
            manager.OperationStarted += Operation_Started;
            manager.OperationCompleted += OperationStopped;

            FtpManagerViewModel ftpManagerViewModel = new FtpManagerViewModel(manager);
            ftpManagerViewModel.LocalRootChanged += FtpManagerViewModel_LocalRootChanged;
            return ftpManagerViewModel;
        }

        private bool ValidateSettings(SourceDestinationPaths _paths)
        {
            Debug.Assert(_paths != null);

            if (!Directory.Exists(_paths.SourcePath))
            {
                MessageBox.Show($"Error: directory {_paths.SourcePath} does not exist.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!Directory.Exists(_paths.DestinationPath))
            {
                MessageBox.Show($"Error: directory {_paths.DestinationPath} does not exist.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void FtpManagerViewModel_LocalRootChanged(object _sender, StringEventArgs _e)
        {
            Settings.Default.LastRemotePath = FtpManagerViewModel.SelectedRemoteRoot;
            Settings.Default.Save();
        }

        private void RunPostOperations()
        {
            FtpManagerViewModel ftpManagerViewModel = FtpManagerViewModel;
            Debug.Assert(ftpManagerViewModel != null);

            SelectLastLocalRoot(ftpManagerViewModel);

            bool autoConnectFtp = Settings.Default.AutoConnectFtp;
            if (autoConnectFtp)
            {
                FtpClient client = ftpManagerViewModel.Manager?.Client;
                Debug.Assert(client != null);

                ICommand connect = client.InvertConnectionCommand;
                if (connect.CanExecute(null))
                {
                    connect.Execute(null);
                }
            }
        }

        private static void SelectLastLocalRoot(FtpManagerViewModel _managerViewModel)
        {
            string lastRemotePath = Settings.Default.LastRemotePath;

            if (!string.IsNullOrWhiteSpace(lastRemotePath))
            {
                _managerViewModel.SelectedRemoteRoot = lastRemotePath;
            }
            else
            {
                Settings.Default.LastRemotePath = _managerViewModel.SelectedLocalRoot;
                Settings.Default.Save();
            }
        }

        public bool CanExit()
        {
            var canExit = true;
            canExit &= FtpManagerViewModel.CanExit();
            canExit &= !CompletedDirectory.IsBusy;
            return canExit;
        }

        public async Task Dispose()
        {
            await FtpManagerViewModel.Dispose();

            Cleanup();
        }

        private void Operation_Started(object _sender, EventArgs _e)
        {
            if (!TaskBarIcon.IsBusy)
            {
                TaskBarIcon.SetIsBusy();
            }
        }

        private void OperationStopped(object _sender, EventArgs _e)
        {
            if (TaskBarIcon.IsBusy)
            {
                TaskBarIcon.SetIsNotBusy();
            }
        }
    }
}
