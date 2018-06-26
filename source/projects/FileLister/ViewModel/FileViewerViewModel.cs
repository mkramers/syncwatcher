using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using FileLister.Model;
using FileLister.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FileLister.ViewModel
{
    public class FileViewerViewModel : ViewModelBase
    {
        public FileViewerViewModel(DirectoriesModel _directoriesModel)
        {
            Debug.Assert(_directoriesModel != null);

            m_directoriesModel = _directoriesModel;

            //get the data for the binding
            m_paths.AddRange(m_directoriesModel.Paths);

            Settings.Default.PropertyChanged += RequestRefreshFilter;
            Settings.Default.SettingChanging += RequestRefreshFilter;

            FileInfoView = CollectionViewSource.GetDefaultView(m_files);
            FileInfoView.Filter = Filter;

            Update();

            new Timer(Tick, null, 0, 10);
        }


        private bool Filter(object _item)
        {
            var row = _item as FileRowViewModel;
            Debug.Assert(row != null);

            var file = row.FileInfo;
            Debug.Assert(file != null);

            var fileName = file.Name.ToLowerInvariant();
            var fileNameWords = fileName.Split(' ').ToList();

            var searchWords = SearchPattern.Split(' ').ToList();

            var match = false;
            var ignoredExtensions = Settings.Default.IgnoredExtensions;
            if (!ignoredExtensions.Contains(file.Extension))
                foreach (var searchWord in searchWords)
                {
                    var fileWordMatches = fileNameWords.Exists(_word => _word.Contains(searchWord));
                    if (!fileWordMatches)
                    {
                        match = false;
                        break;
                    }
                    match = true;
                }

            return match;
        }

        private void Update()
        {
            var fileModels = m_directoriesModel.Files.Select(_file =>
                new FileRowViewModel
                {
                    FileInfo = _file,
                    IsSelected = false
                });

            //update bound items
            m_files.Clear();
            m_files.AddRange(fileModels);

            var previousCount = m_directoriesModel.PreviousFileCount;
            var currentCount = m_directoriesModel.Files.Count;
            SearchResultsSummary = GetUpdateString(FileInfoView, previousCount, currentCount);
        }

        private static void ShowInExplorer(object _params)
        {
            var info = _params as FileSystemInfo;
            Debug.Assert(info != null);

            var isFile = Common.Utilities.IsFile(info);

            var directory = isFile ? Path.GetDirectoryName(info.FullName) : info.FullName;
            Debug.Assert(Directory.Exists(directory));

            Process.Start(directory);
        }

        private void ClearSearch()
        {
            SearchPattern = "";
        }


        //search

        private void Tick(object _sender)
        {
            ProcessRefreshFilterQueue();
        }

        private void ProcessRefreshFilterQueue()
        {
            //called from tick thread
            lock (m_filterQueue)
            {
                if (m_filterQueue.Count > 0)
                {
                    var nextFilter = m_filterQueue[0];
                    m_filterQueue.RemoveAt(0);
                    Debug.Assert(nextFilter != null);

                    var distpatcher = Dispatcher.CurrentDispatcher;
                    distpatcher.BeginInvoke((Action) RefreshFilter);

                    Debug.Assert(m_filterQueue.Count == FILTER_QUEUE_SIZE - 1);
                }
            }
        }

        private void RequestRefreshFilter(object _sender, EventArgs _e)
        {
            //if the queue is at max capcity replace the pending filter with the requested, otherwise process now
            //called from ui thread
            Debug.Assert(m_filterQueue.Count <= FILTER_QUEUE_SIZE);
            lock (m_filterQueue)
            {
                if (m_filterQueue.Count == FILTER_QUEUE_SIZE)
                    m_filterQueue[m_filterQueue.Count - 1] = _e;
                else
                    m_filterQueue.Add(_e);
            }
        }

        private void RefreshFilter()
        {
            var fileViewer = FileInfoView;

            var message = GetResultsString(fileViewer);

            fileViewer.Refresh();
            SearchResultsSummary = message;
        }


        //messaging helpers

        private static string GetUpdateString(IEnumerable _view, int _previousCount, int _currentCount)
        {
            Debug.Assert(_view != null);

            var resultsMessage = GetResultsString(_view);
            var updateMessage = GetUpdateMessage(_currentCount, _previousCount);

            var message = $"{resultsMessage} <{updateMessage}>";
            return message;
        }

        private static string GetUpdateMessage(int _currentCount, int _previousCount)
        {
            var difference = _currentCount - _previousCount;

            var message = "";
            if (difference > 0)
                message = $"{difference} files added";
            else if (difference < 0)
                message = $"{Math.Abs(difference)} files removed";
            else if (difference == 0)
                message = $"{difference} files changed";
            return message;
        }

        private static string GetResultsString(IEnumerable _view)
        {
            Debug.Assert(_view != null);

            var numberOfResults = GetNumberOfResults(_view);

            var result = $"{numberOfResults} results";
            return result;
        }

        private static int GetNumberOfResults(IEnumerable _view)
        {
            Debug.Assert(_view != null);

            var items = _view.Cast<object>();
            Debug.Assert(items != null);

            return items.Count();
        }


        public event Action<object, List<FileRowViewModel>> SelectionChanged;
        public event EventHandler<FileViewerEventArgs> LogEvent;

        private void BrowsePath(object _obj)
        {
            var initialPath = "";

            if (_obj != null)
            {
                var directoryInfo = _obj as DirectoryInfo;
                Debug.Assert(directoryInfo != null);

                initialPath = directoryInfo.FullName;
            }

            //open file dialog
        }

        public RelayCommand UpdateCommand => new RelayCommand(Update);
        public RelayCommand ClearSearchCommand => new RelayCommand(ClearSearch);
        public RelayCommand<object> ShowInExplorerCommand => new RelayCommand<object>(ShowInExplorer);
        public RelayCommand<object> BrowsePathCommand => new RelayCommand<object>(BrowsePath);

        public IEnumerable<DirectoryInfo> Paths => m_paths;
        public DirectoryInfo FirstPath => m_paths.FirstOrDefault();

        public string SearchPattern
        {
            get => m_search;
            set
            {
                if (m_search != value)
                {
                    m_search = value;
                    RequestRefreshFilter(this, new PropertyChangedEventArgs("SearchPattern"));
                    RaisePropertyChanged();
                }
            }
        }

        public string SearchResultsSummary
        {
            get => m_results;
            set
            {
                //if (m_results != value)
                {
                    m_results = value;
                    RaisePropertyChanged();

                    if (LogEvent != null)
                    {
                        var args = new FileViewerEventArgs {Message = m_results};
                        LogEvent(this, args);
                    }
                }
            }
        }

        public ICollectionView FileInfoView { get; }

        public IList<object> Selected
        {
            get => m_selectd;
            set
            {
                m_selectd = value;

                var args = m_selectd.Cast<FileRowViewModel>().ToList();
                SelectionChanged?.Invoke(this, args);

                RaisePropertyChanged();
            }
        }

        private readonly DirectoriesModel m_directoriesModel;

        private readonly ObservableRangeCollection<FileRowViewModel> m_files =
            new ObservableRangeCollection<FileRowViewModel>();

        private readonly List<EventArgs> m_filterQueue = new List<EventArgs>(FILTER_QUEUE_SIZE);

        private readonly ObservableRangeCollection<DirectoryInfo> m_paths =
            new ObservableRangeCollection<DirectoryInfo>();

        private string m_results = "";
        private string m_search = "";
        private IList<object> m_selectd = new List<object>();

        private const int FILTER_QUEUE_SIZE = 1;

        public class FileViewerEventArgs : EventArgs
        {
            public string Message { get; set; }
        }
    }
}