using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.Framework;
using Common.SFTP;
using FileGetter;
using GalaSoft.MvvmLight.Command;
using WinSCP;

namespace WinScpApi.ViewModel
{
    public class FtpDirectoryViewModel : FtpFilesystemItemViewModel
    {
        private bool m_isDownloadedHidden;
        private DirectoryObjectState m_state;

        public RelayCommand MarkSelectedAsDownloadedCommand => new RelayCommand(MarkSelectedAsDownloaded);
        public RelayCommand IsDownloadedHiddenCommand => new RelayCommand(ToggleDownloadedHidden);

        public override string FullName => IsRoot ? Root : DirectoryInfo.FullName;

        public string RelativeDirectory => IsRoot ? FullName : FullName.Substring(Root.Length + 1, FullName.Length - Root.Length - 1);

        public bool IsRoot => DirectoryInfo == null;
        public RemoteFileInfo DirectoryInfo { get; }
        public FtpManager Manager { get; }
        public string Root { get; }

        public DirectoryObjectState State
        {
            get => m_state;
            set
            {
                if (m_state != value)
                {
                    m_state = value;
                    RaisePropertyChanged();

                    if (m_state == DirectoryObjectState.COMPLETED)
                    {
                        FileHistory history = Manager.Cache;
                        history.AddItem(this);
                    }
                }
            }
        }

        public bool IsDownloadedHidden
        {
            get => m_isDownloadedHidden;
            set
            {
                if (m_isDownloadedHidden != value)
                {
                    m_isDownloadedHidden = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FtpDirectoryViewModel(string _root, FtpManager _manager, bool _isRoot) : base(null, !_isRoot)
        {
            Debug.Assert(_manager != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));

            DirectoryInfo = null;
            Manager = _manager;
            Root = _root;
        }

        public FtpDirectoryViewModel(RemoteFileInfo _directoryInfo, FtpDirectoryViewModel _parent, FtpManager _manager, string _root) : base(_parent, _parent != null)
        {
            Debug.Assert(_directoryInfo != null);
            Debug.Assert(_directoryInfo.IsDirectory);
            Debug.Assert(_manager != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));

            DirectoryInfo = _directoryInfo;
            Manager = _manager;
            Root = _root;
        }

        public override FtpFilesystemItemViewModel Find(Func<FtpFilesystemItemViewModel, bool> _func)
        {
            FtpFilesystemItemViewModel match = null;

            IEnumerable<FtpFilesystemItemViewModel> ftpItems = Children.Where(_child => _child is FtpFilesystemItemViewModel).Cast<FtpFilesystemItemViewModel>();
            foreach (FtpFilesystemItemViewModel ftpItem in ftpItems)
            {
                match = ftpItem.Find(_func);
                if (match != null)
                {
                    break;
                }
            }

            return match;
        }

        public override async Task<IEnumerable<FtpFileViewModel>> GetSelectedFiles()
        {
            List<FtpFileViewModel> selectedFiles = new List<FtpFileViewModel>();

            if (IsSelected)
            {
                //force lazy load in case its not expanded...
                await LazyLoad();

                foreach (TreeViewItemViewModel child in Children)
                {
                    child.IsSelected = true;
                }
            }

            IEnumerable<FtpFilesystemItemViewModel> allFtpItems = Children.Where(_child => _child is FtpFilesystemItemViewModel).Cast<FtpFilesystemItemViewModel>();
            foreach (FtpFilesystemItemViewModel ftpItem in allFtpItems)
            {
                TreeViewItemViewModel parent = ftpItem.Parent;
                bool isRoot = parent == null;
                if (isRoot || ftpItem.IsSelected || IsExpanded)
                {
                    IEnumerable<FtpFileViewModel> files = await ftpItem.GetSelectedFiles();

                    selectedFiles.AddRange(files);
                }
            }

            return selectedFiles;
        }

        protected override async Task LoadChildren()
        {
            Children.Clear();

            List<FtpDirectoryViewModel> directories = new List<FtpDirectoryViewModel>();
            List<FtpFileViewModel> files = new List<FtpFileViewModel>();

            RemoteDirectoryInfo directoryInfo = await Manager.Client.GetDirectories(FullName);
            foreach (RemoteFileInfo item in directoryInfo.Files.ToList())
            {
                if (item.IsDirectory)
                {
                    if (item.Name == "." || item.Name == "..")
                    {
                        continue;
                    }

                    FtpDirectoryViewModel viewModel = new FtpDirectoryViewModel(item, this, Manager, Root);
                    Manager.Cache.UpdateStatus(viewModel);
                    directories.Add(viewModel);
                }
                else
                {
                    FileObject fileObject = new FileObject(item, Root);
                    Manager.Cache.UpdateStatus(fileObject);

                    FtpFileViewModel viewModel = new FtpFileViewModel(fileObject, this);
                    files.Add(viewModel);
                }
            }

            foreach (FtpDirectoryViewModel directory in directories)
            {
                if (IsDownloadedHidden)
                {
                    directory.IsDownloadedHidden = IsDownloadedHidden;

                    if (directory.State == DirectoryObjectState.COMPLETED)
                    {
                        continue;
                    }
                }

                Children.Add(directory);
            }
            foreach (FtpFileViewModel file in files)
            {
                if (IsDownloadedHidden && file.File.State == FileObjectState.COMPLETED)
                {
                    continue;
                }

                Children.Add(file);
            }

            RefreshState();
        }

        private async void MarkSelectedAsDownloaded()
        {
            IEnumerable<FtpFileViewModel> selected = await GetSelectedFiles();

            Manager.MarkAsDownloaded(selected);

            await Manager.Refresh();
        }

        public void RefreshState()
        {
            State = GetState(this);

            //update parent
            if (Parent is FtpDirectoryViewModel parentDirectory)
            {
                parentDirectory.RefreshState();
            }
        }

        private async void ToggleDownloadedHidden()
        {
            if (IsRoot)
            {
                await LoadChildren();
            }
        }

        private static DirectoryObjectState GetState(IEnumerable<DirectoryObjectState> _directoryStates)
        {
            Debug.Assert(_directoryStates != null);

            DirectoryObjectState[] directoryObjectStates = _directoryStates as DirectoryObjectState[] ?? _directoryStates.ToArray();
            bool anyNone = directoryObjectStates.Any(_state => _state == DirectoryObjectState.NONE);
            bool anyCompleted = directoryObjectStates.Any(_state => _state == DirectoryObjectState.COMPLETED);
            bool anyInProgress = directoryObjectStates.Any(_state => _state == DirectoryObjectState.IN_PROGRESS);

            DirectoryObjectState state;

            if (anyInProgress)
            {
                state = DirectoryObjectState.IN_PROGRESS;
            }
            else
            {
                if (anyCompleted)
                {
                    state = anyNone ? DirectoryObjectState.INCOMPLETE : DirectoryObjectState.COMPLETED;
                }
                else
                {
                    state = DirectoryObjectState.NONE;
                }
            }
            return state;
        }

        public static DirectoryObjectState GetState(FtpDirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            DirectoryObjectState state;

            FtpFileViewModel[] files = _directory.Children.Where(_child => _child is FtpFileViewModel).Cast<FtpFileViewModel>().ToArray();
            IEnumerable<FileObjectState> fileStates = files.Select(_file => _file.File.State);

            DirectoryObjectState fileState = FileObject.GetState(fileStates);

            FtpDirectoryViewModel[] directories = _directory.Children.Where(_child => _child is FtpDirectoryViewModel).Cast<FtpDirectoryViewModel>().ToArray();
            IEnumerable<DirectoryObjectState> directoryStates = directories.Select(_file => _file.State);

            DirectoryObjectState directoryState = GetState(directoryStates);

            if (!files.Any())
            {
                state = directoryState;
            }
            else if (!directories.Any())
            {
                state = fileState;
            }
            else
            {
                if (fileState == DirectoryObjectState.IN_PROGRESS || directoryState == DirectoryObjectState.IN_PROGRESS)
                {
                    state = DirectoryObjectState.IN_PROGRESS;
                }
                else if (fileState == DirectoryObjectState.COMPLETED)
                {
                    state = directoryState == DirectoryObjectState.COMPLETED ? DirectoryObjectState.COMPLETED : DirectoryObjectState.INCOMPLETE;
                }
                else if (directoryState == DirectoryObjectState.COMPLETED)
                {
                    state = fileState == DirectoryObjectState.COMPLETED ? DirectoryObjectState.COMPLETED : DirectoryObjectState.INCOMPLETE;
                }
                else
                {
                    state = DirectoryObjectState.NONE;
                }
            }

            return state;
        }
    }
}
