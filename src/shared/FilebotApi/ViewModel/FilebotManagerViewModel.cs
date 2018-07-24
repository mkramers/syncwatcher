using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using MVVM.ViewModel;
using System.Collections.Generic;
using Common.IO;

namespace FilebotApi.ViewModel
{
    public class FilebotManagerViewModel : ViewModelBase
    {
        public Filebot Filebot { get; }
        public LocalCleanerViewModel CompletedDirectory { get; }
        public IEnumerable<DirectoryViewModel> Directories { get; }

        public FilebotManagerViewModel(SourceDestinationPaths _paths, IEnumerable<DirectoryViewModel> _directories, Filebot _filebot)
        {
            Debug.Assert(_paths != null);
            Debug.Assert(_directories != null);
            Debug.Assert(_filebot != null);
            
            _filebot.Stopped += Filebot_OnStopped;

            Directories = _directories;

            CompletedDirectory = new LocalCleanerViewModel(_paths, _filebot);
        }

        private void Filebot_OnStopped(object _sender, FileBotOrganizeEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(RefreshCompletedDirectory);
        }

        private void RefreshCompletedDirectory()
        {
            ICommand refreshCommand = CompletedDirectory.DirectoryViewModel.RefreshCommand;
            if (refreshCommand.CanExecute(null))
                refreshCommand.Execute(null);
        }

        public event EventHandler FilebotStarted
        {
            add => Filebot.Started += value;
            remove => Filebot.Started -= value;
        }
        public event EventHandler<FileBotOrganizeEventArgs> FilebotCompleted
        {
            add => Filebot.Stopped += value;
            remove => Filebot.Stopped -= value;
        }
    }
}