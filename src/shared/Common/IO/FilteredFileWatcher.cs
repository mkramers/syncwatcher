using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Common.IO
{
    public class FilteredFileWatcher : FileWatcher
    {
        private readonly List<string> m_ignoredFileNames;

        public FilteredFileWatcher(string _directory, IEnumerable<string> _ignoredFileNames) : base(_directory)
        {
            Debug.Assert(_ignoredFileNames != null);

            m_ignoredFileNames = new List<string>(_ignoredFileNames);
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

                isCompleted = !allPaths.Any(_file => m_ignoredFileNames.Any(_file.Contains));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting files: {e.Message}");

                isCompleted = false;
            }

            return isCompleted;
        }
    }
}
