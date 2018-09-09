using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common.IO;
using MVVM.ViewModel;

namespace SyncWatcherTray.ViewModel
{
    public class SyncthingDirectoryViewModel : DirectoryViewModel
    {
        private readonly FilteredFileWatcher m_filteredFileWatcher;

        /// <summary>
        ///     used only by desginer
        /// </summary>
        public SyncthingDirectoryViewModel()
        {
        }

        public SyncthingDirectoryViewModel(string _directory, string _shortName) : base(_directory, _shortName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_directory));
            Debug.Assert(!string.IsNullOrWhiteSpace(_shortName));
            Debug.Assert(Directory.Exists(_directory));

            List<string> ignoredFileNames = new List<string>
            {
                "~syncthing~"
            };

            m_filteredFileWatcher = new FilteredFileWatcher(_directory, ignoredFileNames);
            m_filteredFileWatcher.WatchEvent += SyncthingWatcher_OnChanged;
            m_filteredFileWatcher.Start();
        }

        public event EventHandler<FileSystemEventArgs> SyncthingCompleted;

        private void SyncthingWatcher_OnChanged(object _sender, FileSystemEventArgs _e)
        {
            SyncthingCompleted?.Invoke(this, _e);
        }
    }
}
