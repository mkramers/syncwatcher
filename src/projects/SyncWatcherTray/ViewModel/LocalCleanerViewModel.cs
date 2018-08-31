using System;
using System.ComponentModel;
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
using SyncWatcherTray.Properties;

namespace SyncWatcherTray.ViewModel
{
    public class LocalCleanerViewModel : ViewModelBase
    {
        private bool m_isPlexScanEnabled;
        private bool m_isBusy;
        private bool m_isAutoCleanEnabled;
        public DirectoryViewModel DirectoryViewModel { get; }
        public Filebot Filebot { get; }
        private string OutputDirectory { get; }
        public SyncthingWatcher SyncthingWatcher { get; }
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

                        if (IsPlexScanEnabled)
                        {
                            await ScanPlex();
                        }

                        OnStopped();
                    },
                    CanExecuteFunc = () => Filebot != null && !IsBusy
                };
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
            get { return m_isPlexScanEnabled; }
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

        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopped;

        /// <summary>
        /// used by designer only
        /// </summary>
        public LocalCleanerViewModel()
        {
            Debug.Assert(IsInDesignMode);
        }

        public LocalCleanerViewModel(SourceDestinationPaths _paths, string _appDataDirectory)
        {
            Debug.Assert(_paths != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));

            string inputDir = _paths.SourcePath;
            string outputDir = _paths.DestinationPath;

            Debug.Assert(!string.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(outputDir));

            DirectoryViewModel = new DirectoryViewModel(inputDir, "Complete");
            OutputDirectory = outputDir;

            Filebot = FilebotHelpers.InitializeFilebot(_appDataDirectory);

            SyncthingWatcher = new SyncthingWatcher(inputDir);
            SyncthingWatcher.WatchEvent += SyncthingWatcher_OnChanged;
            SyncthingWatcher.Start();

            //restore sticky setting
            IsAutoCleanEnabled = Settings.Default.IsAutoCleanEnabled;
            IsPlexScanEnabled = Settings.Default.IsPlexScanEnabled;
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
            ICommand organizeCommand = OrganizeCommand;
            if (organizeCommand.CanExecute(null))
            {
                organizeCommand.Execute(null);
            }
        }
    }
}