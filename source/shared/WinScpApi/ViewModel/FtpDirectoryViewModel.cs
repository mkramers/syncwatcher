using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.SFTP;
using FileGetter;
using GalaSoft.MvvmLight.Command;
using WinSCP;

namespace WinScpApi.ViewModel
{
    public class FtpDirectoryViewModel : FtpFilesystemItemViewModel
    {
        public FtpDirectoryViewModel(string _root, FtpManager _manager, bool _isRoot)
            : base(null, !_isRoot)
        {
            Debug.Assert(_manager != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));

            DirectoryInfo = null;
            Manager = _manager;
            Root = _root;
        }

        public FtpDirectoryViewModel(RemoteFileInfo _directoryInfo, FtpDirectoryViewModel _parent, FtpManager _manager,
            string _root)
            : base(_parent, _parent != null)
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

            var ftpItems = Children.Where(_child => _child is FtpFilesystemItemViewModel)
                .Cast<FtpFilesystemItemViewModel>();
            foreach (var ftpItem in ftpItems)
            {
                match = ftpItem.Find(_func);
                if (match != null)
                    break;
            }

            return match;
        }

        public override async Task<IEnumerable<FtpFileViewModel>> GetSelectedFiles()
        {
            var selectedFiles = new List<FtpFileViewModel>();

            if (IsSelected)
            {
                //force lazy load in case its not expanded...
                await LazyLoad();

                foreach (var child in Children)
                    child.IsSelected = true;
            }

            var allFtpItems = Children.Where(_child => _child is FtpFilesystemItemViewModel)
                .Cast<FtpFilesystemItemViewModel>();
            foreach (var ftpItem in allFtpItems)
            {
                var parent = ftpItem.Parent;
                var isRoot = parent == null;
                if (isRoot || ftpItem.IsSelected || IsExpanded)
                {
                    var files = await ftpItem.GetSelectedFiles();

                    selectedFiles.AddRange(files);
                }
            }

            return selectedFiles;
        }

        protected override async Task LoadChildren()
        {
            Children.Clear();

            var directories = new List<FtpDirectoryViewModel>();
            var files = new List<FtpFileViewModel>();

            var directoryInfo = await Manager.Client.GetDirectories(FullName);
            foreach (var item in directoryInfo.Files.ToList())
                if (item.IsDirectory)
                {
                    if (item.Name == "." || item.Name == "..")
                        continue;

                    var viewModel = new FtpDirectoryViewModel(item, this, Manager, Root);
                    Manager.Cache.UpdateStatus(viewModel);
                    directories.Add(viewModel);
                }
                else
                {
                    var fileObject = new FileObject(item, Root);
                    Manager.Cache.UpdateStatus(fileObject);

                    var viewModel = new FtpFileViewModel(fileObject, this);
                    files.Add(viewModel);
                }

            foreach (var directory in directories)
            {
                if (IsDownloadedHidden)
                {
                    directory.IsDownloadedHidden = IsDownloadedHidden;

                    if (directory.State == DirectoryObjectState.COMPLETED)
                        continue;
                }

                Children.Add(directory);
            }
            foreach (var file in files)
            {
                if (IsDownloadedHidden && file.File.State == FileObjectState.COMPLETED)
                    continue;

                Children.Add(file);
            }

            RefreshState();
        }

        private async void MarkSelectedAsDownloaded()
        {
            var selected = await GetSelectedFiles();

            Manager.MarkAsDownloaded(selected);

            await Manager.Refresh();
        }

        public void RefreshState()
        {
            State = GetState(this);

            //update parent
            if (Parent is FtpDirectoryViewModel parentDirectory)
                parentDirectory.RefreshState();
        }

        private async void ToggleDownloadedHidden()
        {
            if (IsRoot)
                await LoadChildren();
        }

        private static DirectoryObjectState GetState(IEnumerable<DirectoryObjectState> _directoryStates)
        {
            Debug.Assert(_directoryStates != null);

            var directoryObjectStates = _directoryStates as DirectoryObjectState[] ?? _directoryStates.ToArray();
            var anyNone = directoryObjectStates.Any(_state => _state == DirectoryObjectState.NONE);
            var anyCompleted = directoryObjectStates.Any(_state => _state == DirectoryObjectState.COMPLETED);
            var anyInProgress = directoryObjectStates.Any(_state => _state == DirectoryObjectState.IN_PROGRESS);

            DirectoryObjectState state;

            if (anyInProgress)
            {
                state = DirectoryObjectState.IN_PROGRESS;
            }
            else
            {
                if (anyCompleted)
                    state = anyNone ? DirectoryObjectState.INCOMPLETE : DirectoryObjectState.COMPLETED;
                else
                    state = DirectoryObjectState.NONE;
            }
            return state;
        }

        public static DirectoryObjectState GetState(FtpDirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            DirectoryObjectState state;

            var files = _directory.Children.Where(_child => _child is FtpFileViewModel)
                .Cast<FtpFileViewModel>().ToArray();
            var fileStates = files.Select(_file => _file.File.State);

            var fileState = FileObject.GetState(fileStates);

            var directories = _directory.Children.Where(_child => _child is FtpDirectoryViewModel)
                .Cast<FtpDirectoryViewModel>().ToArray();
            var directoryStates = directories.Select(_file => _file.State);

            var directoryState = GetState(directoryStates);

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
                if (fileState == DirectoryObjectState.IN_PROGRESS ||
                    directoryState == DirectoryObjectState.IN_PROGRESS)
                    state = DirectoryObjectState.IN_PROGRESS;
                else if (fileState == DirectoryObjectState.COMPLETED)
                    state = directoryState == DirectoryObjectState.COMPLETED
                        ? DirectoryObjectState.COMPLETED
                        : DirectoryObjectState.INCOMPLETE;
                else if (directoryState == DirectoryObjectState.COMPLETED)
                    state = fileState == DirectoryObjectState.COMPLETED
                        ? DirectoryObjectState.COMPLETED
                        : DirectoryObjectState.INCOMPLETE;
                else
                    state = DirectoryObjectState.NONE;
            }

            return state;
        }

        public RelayCommand MarkSelectedAsDownloadedCommand => new RelayCommand(MarkSelectedAsDownloaded);
        public RelayCommand IsDownloadedHiddenCommand => new RelayCommand(ToggleDownloadedHidden);


        public override string FullName => IsRoot ? Root : DirectoryInfo.FullName;

        public string RelativeDirectory =>
            IsRoot ? FullName : FullName.Substring(Root.Length + 1, FullName.Length - Root.Length - 1);

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
                        var history = Manager.Cache;
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

        private bool m_isDownloadedHidden;
        private DirectoryObjectState m_state;
    }
}