using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using Common.SFTP;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MVVM.ViewModel;
using MVVM.ViewModel.Media;
using SyncWatcherTray.Properties;
using WinScpApi;
using WinScpApi.ViewModel;

namespace SyncWatcherTray.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private MediaDirectoryViewModel m_moviesDirectoryViewModel;
        private MediaDirectoryViewModel m_seriesDirectoryViewModel;
        private int m_selectedTabIndex;

        public int SelectedTabIndex
        {
            get => m_selectedTabIndex;
            set
            {
                m_selectedTabIndex = value;
                RaisePropertyChanged();
            }
        }

        public FtpManagerViewModel FtpManagerViewModel { get; }
        public TaskbarIconViewModel TaskBarIcon { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }
        public MediaDirectoryViewModel SeriesDirectoryViewModel
        {
            get => m_seriesDirectoryViewModel;
            set
            {
                m_seriesDirectoryViewModel = value;
                RaisePropertyChanged();
            }
        }
        public MediaDirectoryViewModel MoviesDirectoryViewModel
        {
            get => m_moviesDirectoryViewModel;
            set
            {
                m_moviesDirectoryViewModel = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SwitchToCompletedCommand => new RelayCommand(() => SelectedTabIndex = 1);

        public RelayCommand SwitchToMoviesCommand => new RelayCommand(() => SelectedTabIndex = 3);

        public RelayCommand SwitchToSeriesCommand => new RelayCommand(() => SelectedTabIndex = 2);

        public MainViewModel()
        {
            TaskBarIcon = new TaskbarIconViewModel();

            Settings settings = Settings.Default;
            settings.SettingChanging += Settings_OnSettingChanging;

            string input = settings.CompletedDirectory;
            string outputDir = settings.MediaRootDirectory;

            SourceDestinationPaths paths = new SourceDestinationPaths(input, outputDir);

            ValidateSettings(paths);

            FtpManagerViewModel = InitializeFtpManager(paths.SourcePath);

            string appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDataDirectory = Path.Combine(appDataRoot, "SyncWatcherTray");

            //create app data dir
            Directory.CreateDirectory(appDataDirectory);

            CompletedDirectory = new LocalCleanerViewModel(paths, appDataDirectory);
            CompletedDirectory.Initialize(paths);
            CompletedDirectory.Started += Operation_Started;
            CompletedDirectory.Stopped += OperationStopped;
        }

        public async void Dispose()
        {
            await Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Settings_OnSettingChanging(object _sender, SettingChangingEventArgs _settingChangingEventArgs)
        {
            Settings settings = _sender as Settings;
            Debug.Assert(settings != null);

            string changedSettingName = _settingChangingEventArgs.SettingName;

            const string completeddirectory = "CompletedDirectory";
            const string mediarootdirectory = "MediaRootDirectory";

            //make sure the settings names havent changed!
            Debug.Assert(settings.Properties.Cast<SettingsProperty>().Any(_prop => _prop.Name == completeddirectory));
            Debug.Assert(settings.Properties.Cast<SettingsProperty>().Any(_prop => _prop.Name == mediarootdirectory));

            switch (changedSettingName)
            {
                case completeddirectory:
                    string input = _settingChangingEventArgs.NewValue as string;
                    Debug.Assert(input != null);

                    if (Directory.Exists(input))
                    {
                        CompletedDirectory.SetDirectory(input);
                    }
                    else
                    {
                        CompletedDirectory.ClearDirectory();
                    }
                    break;
                case mediarootdirectory:
                    string outputDir = _settingChangingEventArgs.NewValue as string;
                    Debug.Assert(outputDir != null);

                    if (Directory.Exists(outputDir))
                    {
                        SetDirectory(outputDir);
                    }
                    else
                    {
                        ClearDirectory();
                    }
                    break;
            }
        }

        private FtpManagerViewModel InitializeFtpManager(string _input)
        {
            SessionConfig ftpSessionConfig = SessionConfig.Default;

            FtpManager manager = new FtpManager(
                ftpSessionConfig,
                new List<string>
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

            bool isValid = true;

            if (!Directory.Exists(_paths.SourcePath))
            {
                MessageBox.Show($"Error: directory {_paths.SourcePath} does not exist.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                isValid = false;
            }

            if (!Directory.Exists(_paths.DestinationPath))
            {
                MessageBox.Show($"Error: directory {_paths.DestinationPath} does not exist.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                isValid = false;
            }

            return isValid;
        }

        private void FtpManagerViewModel_LocalRootChanged(object _sender, EventArgs _e)
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

        private void SetDirectory(string _directoryPath)
        {
            Debug.Assert(_directoryPath != null);
            Debug.Assert(Directory.Exists(_directoryPath));

            string seriesDir = Path.Combine(_directoryPath, "TV Shows");
            SeriesDirectoryViewModel = new MediaDirectoryViewModel(seriesDir, MediaDirectoryType.SERIES);

            string moviesDir = Path.Combine(_directoryPath, "Movies");
            MoviesDirectoryViewModel = new MediaDirectoryViewModel(moviesDir, MediaDirectoryType.MOVIES);
        }

        private void ClearDirectory()
        {
            SeriesDirectoryViewModel = null;
            MoviesDirectoryViewModel = null;
        }

        public bool CanExit()
        {
            bool canExit = true;
            canExit &= FtpManagerViewModel.CanExit();
            canExit &= !CompletedDirectory.IsBusy;
            return canExit;
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

        protected virtual async Task Dispose(bool _disposing)
        {
            if (_disposing)
            {
                await FtpManagerViewModel.Dispose();

                TaskBarIcon.Dispose();
            }
        }

        public void Initialize()
        {
            Settings settings = Settings.Default;

            string outputDir = settings.MediaRootDirectory;

            if (Directory.Exists(outputDir))
            {
                SetDirectory(outputDir);
            }
            else
            {
                ClearDirectory();
            }

            RunPostOperations();
        }
    }
}
