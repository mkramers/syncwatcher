using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FilebotApi.Result;
using GalaSoft.MvvmLight;
using MVVM;
using MVVM.ViewModel;

namespace FilebotApi.ViewModel
{
    public class FilebotManagerViewModel : ViewModelBase
    {
        private FilebotManagerViewModel()
        {
            const string input = @"D:\Unsorted\completed";
            const string outputDir = @"F:\Videos";
            const string tvDir = @"F:\videos\TV Shows";
            const string moviesDir = @"F:\videos\Movies";

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var directory = Path.Combine(appData, "SyncWatcher", "Settings");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string settingsPath = Path.Combine(directory, "settings.xml");
            string recordsPath = Path.Combine(directory, "amclog.txt");

            Directories = new[]
            {
                new DirectoryViewModel(tvDir, "TV"),
                new DirectoryViewModel(moviesDir, "MOVIES"),
            };

            if (Filebot.TryCreate(settingsPath, recordsPath, out var filebot))
            {
                filebot.Stopped += Filebot_Completed;

                Filebot = filebot;

                //var watcher = new SyncthingWatcher(input);
                //watcher.WatchEvent += Watcher_WatchEvent;
            }

            CompletedDirectory = new LocalCleanerViewModel(input, outputDir, filebot);
        }

        public static bool TryCreateFilebotManager(out FilebotManagerViewModel _manager)
        {
            _manager = null;

            try
            {
                _manager = new FilebotManagerViewModel();
            }
            catch (Exception e)
            {
                _manager = null;
                Console.WriteLine($"Failed to create FilebotManagerViewModel: {e.Message}");
            }

            return _manager != null;
        }

        private void Filebot_Completed(object _sender, Filebot.FileBotOrganizeEventArgs _e)
        {
            RefreshCompletedDirectory();
        }

        private void RefreshCompletedDirectory()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var refreshCommand = CompletedDirectory.DirectoryViewModel.RefreshCommand;
                if (refreshCommand.CanExecute(null))
                {
                    refreshCommand.Execute(null);
                }
            });
        }

        public event EventHandler FilebotStarted
        {
            add { Filebot.Started += value; }
            remove { Filebot.Started -= value; }
        }
        public event EventHandler<Filebot.FileBotOrganizeEventArgs> FilebotCompleted
        {
            add { Filebot.Stopped += value; }
            remove { Filebot.Stopped -= value; }
        }

        public Filebot Filebot { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }
        public DirectoryViewModel[] Directories { get; }
    }
}
