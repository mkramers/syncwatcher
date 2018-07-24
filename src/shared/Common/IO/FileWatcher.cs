using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using log4net;

namespace Common.IO
{
    public class FileWatcher : IDisposable
    {
        private const int CHECK_INTERVAL = 1000;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(FileWatcher));

        private readonly Timer m_changedTimer = new Timer(CHECK_INTERVAL);
        protected readonly string m_directory;
        private bool m_changeFlag;
        private bool m_disposed;
        private FileSystemEventArgs m_lastChange;

        public event EventHandler<FileSystemEventArgs> WatchEvent;

        public FileWatcher(string _directory)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_directory));
            Debug.Assert(Directory.Exists(_directory));

            m_directory = _directory;

            FileSystemWatcher watcher = new FileSystemWatcher(m_directory)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;

            m_changedTimer.Elapsed += ChangedTimer_Elapsed;
        }

        public void Start()
        {
            Debug.Assert(!m_changedTimer.Enabled);

            m_changedTimer.Start();
        }

        public void Stop()
        {
            Debug.Assert(m_changedTimer.Enabled);

            m_changedTimer.Stop();
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ChangedTimer_Elapsed(object _sender, ElapsedEventArgs _e)
        {
            //whenever a change occurs we raise the changed flag and store the currentChange args
            //if there is a currentchange, the timer looks and asks if our flag is raised
            //  if it is, the flag is lowered
            //  if it is not, we trigger the changed event the currentchange is cleared
            if (m_lastChange != null)
            {
                if (m_changeFlag)
                {
                    Console.WriteLine("\tChange acknowledged");
                    m_changeFlag = false;
                }
                else
                {
                    Console.WriteLine("Change ended!");
                    OnWatchEvent(_sender, m_lastChange);
                    m_lastChange = null;
                }
            }
        }

        private void Watcher_Changed(object _sender, FileSystemEventArgs _e)
        {
            Console.WriteLine(m_lastChange == null ? "Change occured!" : "Change continues...");

            m_lastChange = _e;
            m_changeFlag = true;
        }

        protected virtual bool RequestIsChangeCompleted()
        {
            return true;
        }

        private void OnWatchEvent(object _sender, FileSystemEventArgs _e)
        {
            Debug.Assert(_e != null);

            if (RequestIsChangeCompleted())
            {
                WatchEvent?.Invoke(_sender, _e);
            }
        }

        // Protected implementation of Dispose pattern.
        private void Dispose(bool _disposing)
        {
            if (m_disposed)
            {
                return;
            }

            if (_disposing)
            {
                m_changedTimer.Dispose();
            }

            m_disposed = true;
        }
    }
}