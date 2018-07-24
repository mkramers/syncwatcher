using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.IO
{
    public class SyncthingWatcher : FileWatcher
    {
        private readonly List<string> m_syncKeywords = new List<string>
        {
            "~syncthing~"
        };

        public SyncthingWatcher(string _directory) : base(_directory)
        {
        }

        protected override bool RequestIsChangeCompleted()
        {
            if (!base.RequestIsChangeCompleted())
            {
                return false;
            }

            bool isCompleted;

            //get all files and check for any that indicate sync in progress
            try
            {
                List<string> files = Directory.GetFiles(m_directory, "*.*", SearchOption.AllDirectories).ToList();
                List<string> directories = Directory.GetDirectories(m_directory, "*", SearchOption.AllDirectories).ToList();
                List<string> allPaths = files.Concat(directories).ToList();

                isCompleted = !allPaths.Any(_file => m_syncKeywords.Any(_file.Contains));
            }
            catch (Exception)
            {
                isCompleted = false;
            }

            return isCompleted;
        }
    }
}