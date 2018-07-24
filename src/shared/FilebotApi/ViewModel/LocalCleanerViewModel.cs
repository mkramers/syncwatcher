using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.IO;
using Common.Mvvm;
using GalaSoft.MvvmLight;
using MVVM;
using MVVM.ViewModel;

namespace FilebotApi.ViewModel
{
    public class LocalCleanerViewModel : ViewModelBase
    {
        public LocalCleanerViewModel(SourceDestinationPaths _paths, Filebot _filebot)
        {
            Debug.Assert(_paths != null);

            var inputDir = _paths.SourcePath;
            var outputDir = _paths.DestinationPath;

            Debug.Assert(!string.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(outputDir));
            Debug.Assert(_filebot != null);

            DirectoryViewModel = new DirectoryViewModel(inputDir, "Complete");
            OutputDirectory = outputDir;
            Filebot = _filebot;
            Filebot.BusyChanged += Filebot_OnBusyChaned;
        }

        private void Filebot_OnBusyChaned(object _sender, EventArgs _e)
        {
            var filebot = _sender as Filebot;
            Debug.Assert(filebot != null);

            var isBusy = filebot.IsBusy;
            DirectoryViewModel.IsBusy = isBusy;
        }

        public ICommand OrganizeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        var input = DirectoryViewModel.Name;
                        var output = OutputDirectory;

                        var fileBot = Filebot;

                        await Task.Run(() => fileBot.Organize(input, output));
                    },
                    CanExecuteFunc = () => Filebot != null && !Filebot.IsBusy
                };
            }
        }

        public DirectoryViewModel DirectoryViewModel { get; }
        public Filebot Filebot { get; }
        public string OutputDirectory { get; }
    }
}