using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.Mvvm;
using Common.SFTP;
using FilebotApi.ViewModel;
using GalaSoft.MvvmLight;
using MVVM.Popups;
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

            const string input = @"D:\Unsorted\completed";

            if (!FilebotManagerViewModel.TryCreateFilebotManager(out var filebotManager))
            {
                var message = $"Failed to create FilebotManager! See log for details.";
                PopupManager.Instance.ShowError(message, "Invalid path!");
                return;
            }

            filebotManager.FilebotStarted += Operation_Started;
            filebotManager.FilebotCompleted += Operation_Completed;

            FilebotManager = filebotManager;

            CreateFtpManager(input, out var ftpManagerViewModel);

            ftpManagerViewModel.PropertyChanged += FtpManagerViewModel_PropertyChanged;

            var ftpManager = ftpManagerViewModel.Manager;
            ftpManager.OperationStarted += Operation_Started;
            ftpManager.OperationCompleted += Operation_Completed;

            FtpManagerViewModel = ftpManagerViewModel;

            RunPostOperations(ftpManagerViewModel);
        }

        private static void CreateFtpManager(string _inputDirectory, out FtpManagerViewModel _managerViewModel)
        {
            FtpSessionConfig sessionConfig = Settings.Default.FtpSessionConfig ?? FtpSessionConfig.Default;

            Settings.Default.FtpSessionConfig = sessionConfig;
            Settings.Default.Save();

            FtpManager manager = new FtpManager(sessionConfig, new List<string> { _inputDirectory });
            _managerViewModel = new FtpManagerViewModel(manager);
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

        private static void RunPostOperations(FtpManagerViewModel _ftpManagerViewModel)
        {
            Debug.Assert(_ftpManagerViewModel != null);
            
            SelectLastLocalRoot(_ftpManagerViewModel);

            var autoConnectFtp = Settings.Default.AutoConnectFtp;
            if (autoConnectFtp)
            {
                var client = _ftpManagerViewModel.Manager?.Client;
                Debug.Assert(client != null);

                var connect = client.InvertConnectionCommand;
                if (connect.CanExecute(null))
                {
                    connect.Execute(null);
                }
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

        private void FtpManagerViewModel_PropertyChanged(object _sender, System.ComponentModel.PropertyChangedEventArgs _e)
        {
            var propertyName = _e.PropertyName;
            var correct = nameof(FtpManagerViewModel.SelectedLocalRoot);
            if (propertyName == correct)
            {
            }
            else if (propertyName == nameof(FtpManagerViewModel.SelectedRemoteRoot))
            {
                Settings.Default.LastRemotePath = FtpManagerViewModel.SelectedRemoteRoot;
                Settings.Default.Save();
            }
        }

        private void Operation_Started(object _sender, EventArgs _e)
        {
            TaskBarIcon.SetIsBusy(true);
        }

        private void Operation_Completed(object _sender, EventArgs _e)
        {
            TaskBarIcon.SetIsBusy(false);
        }

        public ICommand OrganizeCommand => FilebotManager.CompletedDirectory.OrganizeCommand;

        public FtpManagerViewModel FtpManagerViewModel { get; }
        public FilebotManagerViewModel FilebotManager { get; }
        public TaskbarIconViewModel TaskBarIcon { get; }
    }
}
