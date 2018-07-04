using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileLister.Model
{
    public class DirectoriesModel
    {
        public DirectoriesModel(List<string> _paths)
        {
            Debug.Assert(_paths != null);

            Files = new List<FileInfo>();

            Update(_paths);
        }

        public int Update()
        {
            var paths = Paths.Select(_path => _path.FullName);
            return Update(paths);
        }

        private int Update(IEnumerable<string> _paths)
        {
            Debug.Assert(_paths != null);

            PreviousFileCount = Files?.Count ?? 0;

            Paths = _paths.Select(_path => new DirectoryInfo(_path));

            Files.Clear();
            foreach (var path in Paths)
            {
                List<FileInfo> files;
                if (DirectoryModelCache.Instance.TryGetFiles(path.FullName, out files))
                    Files.AddRange(files);
            }

            //if (m_useCache)
            //{
            //    var saved = TrySaveToCache(CacheFile, Files);
            //}

            return Files.Count;
        }

        public List<FileInfo> Files { get; }
        public IEnumerable<DirectoryInfo> Paths { get; private set; }
        public int PreviousFileCount { get; private set; }

        //private bool m_useCache = true;
    }

    public class DirectoryModelCache
    {
        public void Initialize(string _appDataDirectory)
        {
            Debug.Assert(!m_isInitialized);
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));
            Debug.Assert(Directory.Exists(_appDataDirectory));

            m_appDataDirectory = _appDataDirectory;

            m_isInitialized = true;
        }

        public void Add(string _root, List<string> _files)
        {
            Debug.Assert(m_isInitialized);
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));
            Debug.Assert(_files != null);

            var rootInfo = new DirectoryInfo(_root);

            var entry = new Entry(rootInfo, _files);

            //replace if already exists, otherwise add
            var found = m_entries.FindIndex(_entry => _root == _entry.Root);
            if (found != -1)
            {
                m_entries[found] = entry;
            }
            else
            {
                if (m_entries.Count < SIZE)
                {
                    m_entries.Add(entry);
                }
                else
                {
                    Debug.Assert(m_entries.Count > 0);

                    var oldestEntry = m_entries.Aggregate((_current, _entry) =>
                        _entry.LastAccessed < _current.LastAccessed ? _entry : _current);

                    if (!m_entries.Remove(oldestEntry))
                        Debug.Fail("failed to remove oldest entry!");
                }
            }
        }

        public bool TryGetFiles(string _root, out List<FileInfo> _files)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));
            Debug.Assert(Directory.Exists(_root));

            _files = null;

            var success = false;
            var found = false;

            var entry = m_entries.Find(_entry => _entry.Root == _root);
            if (entry != null)
            {
                var rootInfo = new DirectoryInfo(_root);
                if (entry.LastAccessed > rootInfo.LastWriteTime)
                {
                    found = true;

                    var filePaths = entry.Files;
                    _files = filePaths.Select(_path => new FileInfo(_path)).ToList();
                    success = true;
                }
            }

            if (!found)
            {
                success = TryGetFilesFromDisk(_root, out _files);

                if (success && m_useCacheFile)
                    UpdateCacheFile();
            }

            return success;
        }

        private void UpdateCacheFile()
        {
            Debug.Assert(m_useCacheFile);

            if (!TrySaveToCache(CacheFile, m_entries))
                Debug.Fail($"failed to save cache file: {CacheFile}");
        }

        private static bool TryGetFilesFromDisk(string _root, out List<FileInfo> _files)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));

            _files = null;

            var success = false;

            if (Directory.Exists(_root))
            {
                var directoryInfo = new DirectoryInfo(_root);

                var files = directoryInfo.GetFiles(DEFAULT_SEARCH, SEARCH_OPTION);
                _files = files.OrderByDescending(_file => _file.LastWriteTime).ToList();
                success = true;
            }

            return success;
        }

        private static bool TryLoadFromCache(string _cacheFilePath, out List<Entry> _entries)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_cacheFilePath));
            Debug.Assert(Directory.Exists(_cacheFilePath));

            _entries = null;

            var success = false;

            try
            {
                _entries = Common.Utilities.XmlDeserializeObject<List<Entry>>(_cacheFilePath);
                success = true;
            }
            catch (Exception)
            {
                //returns false
            }

            return success;
        }

        private static bool TrySaveToCache(string _cacheFilePath, List<Entry> _entries)
        {
            Debug.Assert(_entries != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_cacheFilePath));

            var success = false;

            try
            {
                Common.Utilities.XmlSerializeObject(_entries, _cacheFilePath);
                Debug.Assert(File.Exists(_cacheFilePath));
                success = true;
            }
            catch (Exception)
            {
                //returns false
            }

            return success;
        }

        public string CacheFile => Path.Combine(m_appDataDirectory, CACHE_FILENAME);

        public static DirectoryModelCache Instance { get; } = new DirectoryModelCache();

        private readonly List<Entry> m_entries = new List<Entry>(SIZE);
        private string m_appDataDirectory;
        private bool m_isInitialized;
        private readonly bool m_useCacheFile = true;

        private const int SIZE = 10;
        private const string DEFAULT_SEARCH = "*.*";
        private const SearchOption SEARCH_OPTION = SearchOption.AllDirectories;
        private const string CACHE_FILENAME = "_cache.xml";

        public class Entry
        {
            public Entry()
            {
            }

            public Entry(FileSystemInfo _rootInfo, List<string> _files)
            {
                Debug.Assert(_rootInfo != null);
                Debug.Assert(_rootInfo.Exists);
                Debug.Assert(_files != null);

                Root = _rootInfo.FullName;
                LastWriteTime = _rootInfo.LastWriteTime;
                Files = _files;

                Touch();
            }

            public void Touch()
            {
                LastAccessed = DateTime.Now;
            }

            public string Root { get; set; }
            public List<string> Files { get; set; }
            public DateTime LastWriteTime { get; set; }
            public DateTime? LastAccessed { get; set; }
        }
    }
}