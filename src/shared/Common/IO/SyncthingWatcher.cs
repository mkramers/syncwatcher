using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.IO
{
    public class SyncthingWatcher : FileWatcher
    {
        public SyncthingWatcher(string _directory) : base(_directory)
        {
        }

        protected override void WatchEventCallback(object _sender, FileSystemEventArgs _e)
        {
            bool inProgress;

            //get all files and check for any that indicate sync in progress
            try
            {
                var files = Directory.GetFiles(m_directory, "*.*", SearchOption.AllDirectories).ToList();
                var directories = Directory.GetDirectories(m_directory, "*", SearchOption.AllDirectories).ToList();
                var allPaths = files.Concat(directories).ToList();

                inProgress = allPaths.Any(_file => m_syncKeywords.Any(_file.Contains));
            }
            catch (Exception)
            {
                inProgress = false;
            }

            if (!inProgress)
                OnWatchEvent(this, _e);
            else
                Log.Debug("syncing...");
        }

        private readonly List<string> m_syncKeywords = new List<string>
        {
            "~syncthing~"
        };
    }
}