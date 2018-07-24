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
using FilebotApi.ViewModel;
using GalaSoft.MvvmLight;
using MVVM.Popups;
using MVVM.ViewModel;
using SyncWatcherTray.Properties;
using WinScpApi;
using WinScpApi.ViewModel;

namespace SyncWatcherTray.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            TaskBarIcon = new TaskbarIconViewModel();

            Settings settings = Settings.Default;

            string input = settings.CompletedDirectory;
            string outputDir = settings.MediaRootDirectory;

            SourceDestinationPaths paths = new SourceDestinationPaths(input, outputDir);

            string appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDataDirectory = Path.Combine(appDataRoot, "SyncWatcher");
            if (!Directory.Exists(appDataDirectory))
            {
                Directory.CreateDirectory(appDataDirectory);
            }

            FilebotManager = InitializeFilebotManager(appDataDirectory, paths);

            FtpManagerViewModel = InitializeFtpManager(paths.SourcePath);

            RunPostOperations();
        }

        private FilebotManagerViewModel InitializeFilebotManager(string _appDataDirectory, SourceDestinationPaths _paths)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));
            Debug.Assert(_paths != null);

            Settings settings = Settings.Default;
            
            DirectoryViewModel[] directories =
            {
                new DirectoryViewModel(settings.SeriesDirectory, "TV"),
                new DirectoryViewModel(settings.MovieDirectory, "MOVIES")
            };

            string settingsPath = Path.Combine(_appDataDirectory, "settings.xml");
            string recordsPath = Path.Combine(_appDataDirectory, "amclog.txt");

            if (FilebotManagerViewModel.TryCreateFilebot(settingsPath, recordsPath, out Filebot filebot))
            {
                Debug.Fail("unhandled filebot load failutre");
            }

            FilebotManagerViewModel filebotManager = new FilebotManagerViewModel(_paths, directories, filebot);
            filebotManager.FilebotStarted += Operation_Started;
            filebotManager.FilebotStopped += OperationStopped;
            return filebotManager;
        }

        private FtpManagerViewModel InitializeFtpManager(string _input)
        {
            Settings defaultSettings = Settings.Default;

            defaultSettings.FtpSessionConfig = defaultSettings.FtpSessionConfig ?? FtpSessionConfig.Default;
            defaultSettings.Save();

            FtpManager manager = new FtpManager(defaultSettings.FtpSessionConfig, new List<string>
            {
                _input
            });
            manager.OperationStarted += Operation_Started;
            manager.OperationCompleted += OperationStopped;

            FtpManagerViewModel ftpManagerViewModel = new FtpManagerViewModel(manager);
            ftpManagerViewModel.LocalRootChanged += FtpManagerViewModel_LocalRootChanged;
            return ftpManagerViewModel;
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
            canExit &= !FilebotManager.Filebot.IsBusy;
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

        public ICommand OrganizeCommand => FilebotManager.CompletedDirectory.OrganizeCommand;

        public FtpManagerViewModel FtpManagerViewModel { get; }
        public FilebotManagerViewModel FilebotManager { get; }
        public TaskbarIconViewModel TaskBarIcon { get; }
    }
}
