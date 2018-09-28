using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using Common.Logging;
using FilebotApi;
using FilebotApi.Result;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PlexTools;
using SyncWatcherTray.Properties;
using FilebotSettings = FilebotApi.Properties.Settings;

namespace SyncWatcherTray.ViewModel
{
    public class LocalCleanerViewModel : ViewModelBase, IDisposable
    {
        private SyncthingDirectoryViewModel m_directoryViewModel;
        private bool m_isAutoCleanEnabled;
        private bool m_isBusy;
        private bool m_isPlexScanEnabled;

        public Filebot Filebot { get; }
        public FilebotHistory FilebotHistory { get; }
        private string OutputDirectory { get; }
        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                if (m_isBusy != value)
                {
                    m_isBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand AutoCleanCommand
        {
            get
            {
                async void Execute()
                {
                    string input = DirectoryViewModel.Name;
                    string output = OutputDirectory;

                    string message = $"Starting cleaning...\n  Source: {input}\n  Destination: {output}\n  {new string('=', 24)}";

                    Log.Write(LogLevel.INFO, message);

                    OnStarted();

                    await Organize();

                    if (IsPlexScanEnabled)
                    {
                        await ScanPlex();
                    }

                    Log.Write(LogLevel.INFO, $"  {new string('=', 24)}\nCleaning complete.\n***\n");

                    OnStopped();
                }

                return new RelayCommand(Execute, CanAutoClean);
            }
        }
        public bool IsAutoCleanEnabled
        {
            get => m_isAutoCleanEnabled;
            set
            {
                if (m_isAutoCleanEnabled != value)
                {
                    m_isAutoCleanEnabled = value;
                    RaisePropertyChanged();

                    //save setting
                    Settings.Default.IsAutoCleanEnabled = value;
                    Settings.Default.Save();
                }
            }
        }
        public bool IsPlexScanEnabled
        {
            get => m_isPlexScanEnabled;
            set
            {
                if (m_isPlexScanEnabled != value)
                {
                    m_isPlexScanEnabled = value;
                    RaisePropertyChanged();

                    //save settings
                    Settings.Default.IsPlexScanEnabled = value;
                    Settings.Default.Save();
                }
            }
        }
        public SyncthingDirectoryViewModel DirectoryViewModel
        {
            get => m_directoryViewModel;
            set
            {
                m_directoryViewModel = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     used by designer only
        /// </summary>
        public LocalCleanerViewModel()
        {
            Debug.Assert(IsInDesignMode);
        }

        public LocalCleanerViewModel(SourceDestinationPaths _paths, string _appDataDirectory)
        {
            Debug.Assert(_paths != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));

            string outputDir = _paths.DestinationPath;

            Debug.Assert(!string.IsNullOrWhiteSpace(outputDir));

            OutputDirectory = outputDir;

            FilebotSettings settings = FilebotSettings.Default;

            //hardcode filepaths
            settings.LogFilePath = Path.Combine(_appDataDirectory, "amc_log.txt");
            settings.ExcludeListPath = Path.Combine(_appDataDirectory, "amc_exclude.txt");
            settings.Save();

            Filebot = new Filebot(settings);
            Filebot.FileOrganized += Filebot_OnFileOrganized;

            string histroyPath = Path.Combine(_appDataDirectory, "history.json");

            FilebotHistory = new FilebotHistory();

            try
            {
                FilebotHistory.Load(histroyPath);
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.INFO, $"Error loading history file: {e.Message}");

                FilebotHistory.Save();
                FilebotHistory.Reload();
            }

            //restore sticky setting
            m_isAutoCleanEnabled = Settings.Default.IsAutoCleanEnabled;
            m_isPlexScanEnabled = Settings.Default.IsPlexScanEnabled;
        }

        private void Filebot_OnFileOrganized(object _sender, RenameResultEventArgs _e)
        {
            RenameResult result = _e.Result;
            Debug.Assert(result != null);

            Application.Current.Dispatcher.Invoke(() => FilebotHistory.AddEntry(result));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopped;

        public void SetDirectory(string _directoryPath)
        {
            Debug.Assert(_directoryPath != null);
            Debug.Assert(Directory.Exists(_directoryPath));

            DirectoryViewModel = new SyncthingDirectoryViewModel(_directoryPath, "Complete");
            DirectoryViewModel.SyncthingCompleted += SyncthingWatcher_OnChanged;
        }

        public void ClearDirectory()
        {
            SyncthingDirectoryViewModel currentViewModel = DirectoryViewModel;
            if (currentViewModel != null)
            {
                currentViewModel.SyncthingCompleted -= SyncthingWatcher_OnChanged;
            }

            DirectoryViewModel = null;
        }

        private bool CanAutoClean()
        {
            bool canOrganize = Filebot != null && !IsBusy && DirectoryViewModel != null;

            return canOrganize;
        }

        private async Task Organize()
        {
            string input = DirectoryViewModel.Name;
            string output = OutputDirectory;

            Filebot fileBot = Filebot;

            await Task.Run(() => fileBot.Organize(input, output));
        }

        private async Task ScanPlex()
        {
            Debug.Assert(IsPlexScanEnabled);

            uint[] sections = { 4, 5, 6 };

            Log.Write(LogLevel.INFO, "Starting Plex scan...");

            try
            {
                await Task.Factory.StartNew(() => PlexScanner.ScanSections(sections));

                Log.Write(LogLevel.INFO, "Plex scan completed.");
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.INFO, $"Error scanning plex: {e.Message}");
            }
        }

        private void OnStarted()
        {
            IsBusy = true;

            Started?.Invoke(this, EventArgs.Empty);
        }

        private void OnStopped()
        {
            IsBusy = false;

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void SyncthingWatcher_OnChanged(object _sender, FileSystemEventArgs _e)
        {
            if (!IsAutoCleanEnabled)
            {
                return;
            }

            //perform orangize on changes
            ICommand autoCleanCommand = AutoCleanCommand;
            if (autoCleanCommand.CanExecute(null))
            {
                autoCleanCommand.Execute(null);
            }
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                DirectoryViewModel?.Dispose();
            }
        }

        public void Initialize(SourceDestinationPaths _paths)
        {
            Debug.Assert(_paths != null);

            string inputDir = _paths.SourcePath;

            if (string.IsNullOrWhiteSpace(inputDir) || !Directory.Exists(inputDir))
            {
                ClearDirectory();
            }
            else
            {
                SetDirectory(inputDir);
            }
        }
    }
}
