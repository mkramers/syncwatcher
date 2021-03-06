﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common.IO;
using MVVM.ViewModel;

namespace SyncWatcherTray.ViewModel
{
    public class SyncthingDirectoryViewModel : DirectoryViewModel
    {
        /// <summary>
        ///     used only by desginer
        /// </summary>
        public SyncthingDirectoryViewModel()
        {
        }

        public SyncthingDirectoryViewModel(string _directory, string _shortName) : base(_directory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_directory));
            Debug.Assert(!string.IsNullOrWhiteSpace(_shortName));
            Debug.Assert(Directory.Exists(_directory));

            List<string> ignoredFileNames = new List<string>
            {
                "~syncthing~"
            };

            FilteredFileWatcher filteredFileWatcher = new FilteredFileWatcher(_directory, ignoredFileNames);
            filteredFileWatcher.WatchEvent += SyncthingWatcher_OnChanged;
            filteredFileWatcher.Start();
        }

        public event EventHandler<FileSystemEventArgs> SyncthingCompleted;

        private void SyncthingWatcher_OnChanged(object _sender, FileSystemEventArgs _e)
        {
            SyncthingCompleted?.Invoke(this, _e);
        }
    }
}
