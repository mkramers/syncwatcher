using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;

namespace FilebotApi.ViewModel
{
    public class FilebotManagerViewModel : ViewModelBase
    {
        public Filebot Filebot { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }
        public DirectoryViewModel[] Directories { get; }

        public FilebotManagerViewModel()
        {
            const string input = @"D:\Unsorted\completed";
            const string outputDir = @"F:\Videos";
            const string tvDir = @"F:\videos\TV Shows";
            const string moviesDir = @"F:\videos\Movies";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string directory = Path.Combine(appData, "SyncWatcher", "Settings");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string settingsPath = Path.Combine(directory, "settings.xml");
            string recordsPath = Path.Combine(directory, "amclog.txt");

            Directories = new[]
            {
                new DirectoryViewModel(tvDir, "TV"),
                new DirectoryViewModel(moviesDir, "MOVIES")
            };

            if (Filebot.TryCreate(settingsPath, recordsPath, out Filebot filebot))
            {
                filebot.Stopped += Filebot_Completed;

                Filebot = filebot;
            }

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