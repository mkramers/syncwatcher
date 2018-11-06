using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace MVVM.ViewModel
{
    public class DirectoryViewModel : ViewModelBase, IDisposable
    {
        private bool m_isBusy;
        public FileWatcher FileWatcher { get; }

        public string Name { get; }

        public ICommand RefreshCommand
        {
            get { return new RelayCommand(async () => { await Update(); }); }
        }

        public ObservableRangeCollection<FileInfo> FileNames { get; }
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

        /// <summary>
        ///     used only by desginer
        /// </summary>
        public DirectoryViewModel()
        {
            Debug.Assert(IsInDesignMode);
        }

        public DirectoryViewModel(string _directory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_directory));
            Debug.Assert(Directory.Exists(_directory));

            Name = _directory;

            FileNames = new ObservableRangeCollection<FileInfo>();

            FileWatcher = new FileWatcher(_directory);
            FileWatcher.WatchEvent += FileWatcher_OnWatchEvent;
            FileWatcher.Start();

            ICommand refresh = RefreshCommand;
            if (refresh.CanExecute(null))
            {
                refresh.Execute(null);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void FileWatcher_OnWatchEvent(object _sender, FileSystemEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    ICommand refresh = RefreshCommand;
                    if (refresh.CanExecute(null))
                    {
                        refresh.Execute(null);
                    }
                });
        }

        private async Task Update()
        {
            string directory = Name;
            Debug.Assert(!string.IsNullOrWhiteSpace(directory));

            IsBusy = true;

            if (Directory.Exists(directory))
            {
                IOrderedEnumerable<FileInfo> files = await Task.Factory.StartNew(
                    () =>
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                        FileInfo[] info = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                        IEnumerable<FileInfo> filtered = info.Where(_file => _file.Length > 1000000);
                        IOrderedEnumerable<FileInfo> sorted = filtered.OrderByDescending(_file => _file.CreationTime);
                        return sorted;
                    });

                FileNames.Clear();
                FileNames.AddRange(files);
            }
            else
            {
                FileNames.Clear();
            }

            Updated?.Invoke(this, EventArgs.Empty);

            IsBusy = false;
        }

        public event EventHandler<EventArgs> Updated;

        private void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                FileWatcher.Dispose();
            }
        }
    }
}
