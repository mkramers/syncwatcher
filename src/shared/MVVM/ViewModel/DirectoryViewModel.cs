using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.IO;
using Common.Mvvm;
using GalaSoft.MvvmLight;

namespace MVVM.ViewModel
{
    public class DirectoryViewModel : ViewModelBase
    {
        public FileWatcher FileWatcher { get; }
        
        /// <summary>
        /// used only by desginer
        /// </summary>
        public DirectoryViewModel()
        {
            Debug.Assert(IsInDesignMode);
        }

        public DirectoryViewModel(string _directory, string _shortName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_directory));
            Debug.Assert(!string.IsNullOrWhiteSpace(_shortName));
            Debug.Assert(Directory.Exists(_directory));

            Name = _directory;
            ShortName = _shortName;

            FileNames = new ObservableRangeCollection<FileInfo>();

            FileWatcher= new FileWatcher(_directory);
            FileWatcher.WatchEvent += FileWatcher_OnWatchEvent;
            FileWatcher.Start();

            ICommand refresh = RefreshCommand;
            if (refresh.CanExecute(null))
            {
                refresh.Execute(null);
            }
        }

        private void FileWatcher_OnWatchEvent(object _sender, FileSystemEventArgs _e)
        {
            Application.Current.Dispatcher.Invoke(() =>
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
            var directory = Name;
            Debug.Assert(!string.IsNullOrWhiteSpace(directory));

            IsBusy = true;
            
            if (Directory.Exists(directory))
            {
                var files = await Task.Factory.StartNew(() =>
                {
                    var directoryInfo = new DirectoryInfo(directory);

                    var info = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                    var filtered = info.Where(_file => _file.Length > 1000000);
                    var sorted = filtered.OrderByDescending(_file => _file.CreationTime);
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

        public string Name { get; }
        public string ShortName { get; }

        public ICommand RefreshCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        await Update();
                    }
                };
            }
        }
        public event EventHandler<EventArgs> Updated;

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

        private bool m_isBusy;
    }
}