using System;
using System.Collections.Generic;
using System.Diagnostics;
using FileGetter;
using WinScpApi.ViewModel;

namespace Common.SFTP
{
    public class FileHistory
    {
        public static bool TryLoad(string _filePath, out FileHistory _history)
        {
            _history = Utilities.XmlDeserializeObject<FileHistory>(_filePath);
            return _history != null;
        }

        public static FileHistory LoadOrCreate(string _fileName)
        {
            if (!TryLoad(_fileName, out var history))
            {
                history = new FileHistory();
                //Log.Info("No valid cache file!");
            }

            return history;
        }

        public void Save(string _filePath)
        {
            lock (m_lock)
            {
                Utilities.XmlSerializeObject(this, _filePath);
            }
        }

        public void AddItem(FtpFilesystemItemViewModel _file)
        {
            Debug.Assert(_file != null);

            var newCompletedTime = DateTime.Now;
            var existing = Entries.Find(_entry => _entry.FullName == _file.FullName);
            if (existing != null)
            {
                existing.UpdateCompletedTime(newCompletedTime);
            }
            else
            {
                existing = new FileHistoryEntry(_file.FullName, newCompletedTime);
                Entries.Add(existing);
            }
        }

        public void UpdateStatus(FtpDirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            foreach (var entry in Entries)
                if (string.Equals(_directory.FullName, entry.FullName,
                    StringComparison.InvariantCultureIgnoreCase))
                    _directory.State = DirectoryObjectState.COMPLETED;
        }

        public void UpdateStatus(FileObject _file)
        {
            Debug.Assert(_file != null);
            foreach (var entry in Entries)
                if (string.Equals(_file.FullName, entry.FullName,
                    StringComparison.InvariantCultureIgnoreCase))
                    _file.State = FileObjectState.COMPLETED;
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public List<FileHistoryEntry> Entries { get; } = new List<FileHistoryEntry>();

        private readonly object m_lock = new object();
    }

    public class FileHistoryEntry
    {
        public FileHistoryEntry()
        {
        }

        public FileHistoryEntry(string _fullName, DateTime _completedTime)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_fullName));

            FullName = _fullName;
            CompletedTime = _completedTime;
        }

        public void UpdateCompletedTime(DateTime _newTime)
        {
            CompletedTime = _newTime;
        }

        public string FullName { get; set; }
        public DateTime CompletedTime { get; set; }
    }
}