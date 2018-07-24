using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using Common.Mvvm;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;

namespace FilebotApi.ViewModel
{
    public class LocalCleanerViewModel : ViewModelBase
    {
        public DirectoryViewModel DirectoryViewModel { get; }
        public Filebot Filebot { get; }
        private string OutputDirectory { get; }
        public FileWatcher FileWatcher { get; }

        public ICommand OrganizeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        string input = DirectoryViewModel.Name;
                        string output = OutputDirectory;

                        Filebot fileBot = Filebot;

                        await Task.Run(() => fileBot.Organize(input, output));
                    },
                    CanExecuteFunc = () => Filebot != null && !Filebot.IsBusy
                };
            }
        }

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
            Filebot.Stopped += Filebot_OnStopped;
            Filebot.BusyChanged += Filebot_OnBusyChanged;

            FileWatcher = new FileWatcher(inputDir);
            FileWatcher.WatchEvent += FileWatcher_WatchEvent;
            FileWatcher.Start();
        }

        private void Filebot_OnStopped(object _sender, FileBotOrganizeEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);
        }

        private void RefreshCompletedDirectory()
        {
            ICommand refreshCommand = DirectoryViewModel.RefreshCommand;
            if (refreshCommand.CanExecute(null))
                refreshCommand.Execute(null);
        }

        private void Filebot_OnBusyChanged(object _sender, EventArgs _e)
        {
            Filebot filebot = _sender as Filebot;
            Debug.Assert(filebot != null);

            bool isBusy = filebot.IsBusy;
            DirectoryViewModel.IsBusy = isBusy;
        }

        private void FileWatcher_WatchEvent(object _sender, FileSystemEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);
        }
    }
}