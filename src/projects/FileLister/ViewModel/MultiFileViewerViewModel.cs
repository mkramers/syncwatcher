using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FileLister.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FileLister.ViewModel
{
    public class MultiFileViewerViewModel : ViewModelBase
    {
        public MultiFileViewerViewModel(MultiFileLister _model)
        {
            if (IsInDesignMode)
                return;

            Debug.Assert(_model != null);

            var consoleInfo = $"{DateTime.Now.ToShortTimeString()} [{DateTime.Now.ToShortDateString()}]";
            m_consoleOutput.Add(consoleInfo);

            MultiFileLister = _model;
            MultiFileLister.FileBotLogUpdated += MultiFileLister_Event;
            MultiFileLister.FileBotCompleted += MultiFileLister_FileBotCompleted;

            RenamerViewModel = new RenamerViewModel();

            FileViewers = new List<FileViewerViewModel>(2);
            Debug.Assert(FileViewers.Capacity == MultiFileLister.FileListers.Count);

            foreach (var fileLister in MultiFileLister.FileListers)
            {
                var viewModel = new FileViewerViewModel(fileLister);
                viewModel.LogEvent += FileViewer_LogEvent;
                viewModel.SelectionChanged += FileViewerViewModel_SelectionChanged;

                FileViewers.Add(viewModel);
            }
        }

        private void MultiFileLister_Event(object _sender, EventArgs _e)
        {
            WriteToLog("MultiFileLister_Event BROKEN");
        }

        private void MultiFileLister_FileBotCompleted(object _sender, EventArgs _e)
        {
            UpdateViewModels();
        }

        private void FileViewerViewModel_SelectionChanged(object _sender, List<FileRowViewModel> _e)
        {
            if (_sender == FileViewers[0])
            {
                var fileNames = _e.Select(_file => _file.FileInfo.FullName);
                var list = new ObservableRangeCollection<string>(fileNames);
                RenamerViewModel.InputItems = list;
            }
        }

        private void FileViewer_LogEvent(object _sender, FileViewerViewModel.FileViewerEventArgs _e)
        {
            WriteToLog(_e.Message);
        }


        private void OrganizeDirectory()
        {
            var input = MultiFileLister.FileListers[0].Paths.First().FullName;
            var output = MultiFileLister.FileListers[1].Paths.First().FullName;

            MultiFileLister.Organize(input, output);
        }

        private void UpdateViewModels()
        {
            foreach (var viewer in FileViewers)
                if (viewer.UpdateCommand.CanExecute(null))
                    viewer.UpdateCommand.Execute(null);
        }

        private void WriteToLog(string _line)
        {
            Debug.Assert(_line != null);

            Application.Current.Dispatcher.BeginInvoke((Action) (() =>
            {
                var formattedLine = $">> {_line}";
                ConsoleOutput.Add(formattedLine);
            }));
        }


        public RelayCommand OrganizeCommand => new RelayCommand(OrganizeDirectory);

        public RenamerViewModel RenamerViewModel { get; set; }

        public ObservableCollection<string> ConsoleOutput
        {
            get => m_consoleOutput;
            set
            {
                m_consoleOutput = value;
                RaisePropertyChanged();
            }
        }

        public List<FileViewerViewModel> FileViewers { get; set; }
        public MultiFileLister MultiFileLister { get; set; }

        public string Address
        {
            get => m_address;
            set
            {
                m_address = value;
                RaisePropertyChanged();
            }
        }

        private string m_address;

        private ObservableCollection<string> m_consoleOutput = new ObservableCollection<string>();
    }
}