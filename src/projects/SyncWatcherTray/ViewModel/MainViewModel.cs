using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.Framework.EventHelpers;
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

            const string completedDir = @"D:\Unsorted\completed";

            if (!FilebotManagerViewModel.TryCreateFilebotManager(out var filebotManager))
            {
                var message = $"Failed to create FilebotManager! See log for details.";
                PopupManager.Instance.ShowError(message, "Invalid path!");
                return;
            }

            filebotManager.FilebotStarted += Operation_Started;
            filebotManager.FilebotCompleted += Operation_Completed;

            FilebotManager = filebotManager;

            FtpSessionConfig sessionConfig = Settings.Default.FtpSessionConfig ?? FtpSessionConfig.Default;

            Settings.Default.FtpSessionConfig = sessionConfig;
            Settings.Default.Save();

            FtpManager manager = new FtpManager(sessionConfig, new List<string> { completedDir });
            manager.OperationStarted += Operation_Started;
            manager.OperationCompleted += Operation_Completed;

            FtpManagerViewModel.LocalRootChanged += FtpManagerViewModel_LocalRootChanged;

            RunPostOperations();
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
            TaskBarIcon.SetIsBusy();
        }

        private void Operation_Completed(object _sender, EventArgs _e)
        {
            TaskBarIcon.SetIsNotBusy();
        }

        public ICommand OrganizeCommand => FilebotManager.CompletedDirectory.OrganizeCommand;

        public FtpManagerViewModel FtpManagerViewModel { get; }
        public FilebotManagerViewModel FilebotManager { get; }
        public TaskbarIconViewModel TaskBarIcon { get; }
    }
}
