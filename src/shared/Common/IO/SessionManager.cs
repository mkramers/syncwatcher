//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WinSCP;

//namespace Common
//{
//    public class SessionManager
//    {
//        public SessionManager()
//        {
//            m_remoteTree = new Tree<RemoteFileInfo>();
//            m_ignoreTree = new Tree<RemoteFileInfo>();

//            Stopwatch = new Stopwatch();
//        }

//        public int OpenSession(string _remoteRoot, string _localPath, string _ignoreCache)
//        {
//            RemoteRoot = _remoteRoot;
//            IgnoreCache = _ignoreCache;
//            LocalRoot = _localPath;

//            m_sessionInfo = new SessionInfo();
//            m_sessionInfo.Load();

//            try
//            {
//                m_session = new Session();
//                m_session.ExecutablePath = "../../../External/winscp573automation/WinSCP.exe";
//                m_session.Open(m_sessionInfo.SessionOptions);

//                LOG.DebugFormat("Opened session at {0} with default settings", m_sessionInfo.SessionOptions.HostName);

//                return 0;
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//                return 1;
//            }
//        }

//        public void UpdateRemoteFileList() 
//        {
//            Stopwatch.Restart();

//            m_remoteTree.Data = new RemoteFileInfo();
//            m_remoteTree.Data.Name = RemoteRoot;

//            Update(RemoteRoot, m_remoteTree);

//            if (PrintTrees)
//            {
//                PrintRemoteChildren();
//            }
//            else
//            {
//                int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_remoteTree, 0);
//                numberOfFiles = (numberOfFiles == 1) ? 0 : numberOfFiles;

//                LOG.DebugFormat("Refreshed {0} remote files in [{1}ms]", numberOfFiles, Stopwatch.ElapsedMilliseconds);
//            }
//        }

//        private void Update(string _path, Tree<RemoteFileInfo> _tree)
//        {
//            try
//            {
//                RemoteDirectoryInfo directory = m_session.ListDirectory(_path);

//                foreach (WinSCP.RemoteFileInfo fileInfo in directory.Files)
//                {
//                    if (fileInfo.Name != "..")
//                    {
//                        if (fileInfo.IsDirectory)
//                        {
//                            //if the entry not cached - add the directory and then update its contents
//                            if (Tree<RemoteFileInfo>.Find(m_ignoreTree, i => i.Name == fileInfo.Name && i.IsDirectory) == null)
//                            {
//                                _tree.AddChild(new RemoteFileInfo(fileInfo));

//                                Update(_path + "/" + fileInfo.Name, _tree.GetChild(_tree.Children.Count - 1));
//                            }
//                        }
//                        else
//                        {
//                            //if we are not ignoring the file
//                            if (Tree<RemoteFileInfo>.Find(m_ignoreTree, i => i.Name == fileInfo.Name && !i.IsDirectory) == null)
//                            {
//                                //if the file extension is one we care about add it to list, other wise add to the ignore list
//                                //if (!m_ignoredExtensions.Contains(Utilities.GetExtensionFromFileName(fileInfo.Name)))
//                                {
//                                    _tree.AddChild(new RemoteFileInfo(fileInfo));
//                                }
//                                //else
//                                //{
//                                //    m_ignoreTree.AddChild(new RemoteFileInfo2(fileInfo));
//                                //}
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }
//        }

//        public void SyncRemoteFilesWithLocal(bool _remove = false, TransferOptions _transformOptions = null)
//        {
//            Stopwatch.Restart();

//            m_ignoreTree = new Tree<RemoteFileInfo>(m_remoteTree.Data);

//            Tree<RemoteFileInfo> tempIgnoreTree = new Tree<RemoteFileInfo>();

//            Sync(m_remoteTree, tempIgnoreTree, RemoteRoot, LocalRoot, _remove, _transformOptions);

//            LOG.DebugFormat("Synced {0} files in {1} ms", m_syncedFiles, Stopwatch.ElapsedMilliseconds);
//        }

//        public void Sync(Tree<RemoteFileInfo> _tree, Tree<RemoteFileInfo> _tempIgnoreTree, string _remotePath, string _localPath, bool _remove = false, TransferOptions _transformOptions = null)
//        {
//            try
//            {
//                if (_tree.Children.Count != 0)
//                {
//                    for (int i = 0; i < _tree.Children.Count; i++)
//                    {
//                        if (_tree.Children[i] != null)
//                        {
//                            if (_tree.Children[i].Children != null)
//                            {
//                                string data = _tree.Children[i].Data.ToString();

//                                if (data == null)
//                                {
//                                    data = string.Empty;
//                                }

//                                Tree<RemoteFileInfo> node = _tree.Children[i];
//                                _tempIgnoreTree.AddNode(node);

//                                Console.WriteLine("AT -> {0}", node.Data);

//                                Sync(_tree.Children[i], _tempIgnoreTree.Children[_tempIgnoreTree.Children.Count - 1], _remotePath + "/" + data, _localPath, _remove, _transformOptions);
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    if (_tree.Data != null)
//                    {
//                        string data = _tree.Data.ToString();
//                        if (data != null)
//                        {
//                            string source = _remotePath + "/" + data;
//                            if (!TestMode)
//                            {
//                                GetFiles(source, _localPath, _remove, _transformOptions);
//                            }
//                            else
//                            {
//                                //Console.WriteLine("Syncing {0} to {1}", source, _localPath);
//                            }

//                            Console.WriteLine("-------------DUMP--------------");
//                            Tree<RemoteFileInfo>.PrintTree(_tempIgnoreTree, 0);
//                            Console.WriteLine("--------------END--------------");

//                            m_ignoreTree.AddNode(Tree<RemoteFileInfo>.Clone(_tempIgnoreTree));

//                            _tempIgnoreTree = null;
//                            _tempIgnoreTree = new Tree<RemoteFileInfo>();

//                            m_syncedFiles++;
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }
//        }

//        private void GetFiles(string _remotePath, string _localPath, bool _remove = false, TransferOptions _transformOptions = null)
//        {
//            LOG.DebugFormat("Getting files from {0} >> {1}", _remotePath, _localPath);

//            try
//            {
//                // Download files and throw on any error
//                m_session.GetFiles(_remotePath, _localPath, _remove, _transformOptions).Check();
//            }
//            finally
//            {
//                // Terminate line after the last file (if any)
//                if (m_lastFileName != null)
//                {
//                    LOG.Info("\nGetFiles done!");
//                }
//            }
//        }

//        public void AddAllRemoteFilesToIgnoreList()
//        {
//            LOG.DebugFormat("Adding all remote files from {0} to ignore list", RemoteRoot);

//            m_ignoreTree = Tree<RemoteFileInfo>.Clone(m_remoteTree);
//        }

//        public void SaveIgnoreTree()
//        {
//            LOG.DebugFormat("Saving ignore tree to {0}", IgnoreCache);

//            try
//            {
//                Utilities.XmlSerializeObject<Tree<RemoteFileInfo>>(m_ignoreTree, IgnoreCache);
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }
//        }

//        public void LoadIgnoreTree()
//        {
//            Stopwatch.Restart();

//            try
//            {
//                m_ignoreTree = Utilities.XmlDeserializeObject<Tree<RemoteFileInfo>>(IgnoreCache);
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }

//            if (m_ignoreTree == null)
//            {
//                LOG.WarnFormat("Ignore tree at {0} is not available", IgnoreCache);
//            }

//            if (PrintTrees)
//            {
//                PrintIgnoreChildren();
//            }
//            else
//            {
//                int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_ignoreTree, 0);
//                LOG.DebugFormat("Loaded {0} ignored files", numberOfFiles);
//            }
//        }

//        public void PrintRemoteTree()
//        {
//            LOG.Debug("Listing remote tree...");

//            try
//            {
//                Tree<RemoteFileInfo>.PrintTree(m_remoteTree, 0);
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }

//            int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_remoteTree, 0);
//            LOG.DebugFormat("Refreshed {0} remote files", numberOfFiles);
//        }
//        public void PrintRemoteChildren()
//        {
//            LOG.Debug("Listing remote files...");

//            try
//            {
//                if (m_remoteTree.Children.Count > 0)
//                {
//                    Tree<RemoteFileInfo>.PrintTreeChildren(m_remoteTree, "");
//                }
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }

//            int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_remoteTree, 0);
//            numberOfFiles = (numberOfFiles == 1) ? 0 : numberOfFiles;

//            LOG.DebugFormat("Refreshed {0} remote files", numberOfFiles);
//        }
//        public void PrintIgnoreTree()
//        {
//            LOG.Debug("Listing ignored files...");

//            try
//            {
//                Tree<RemoteFileInfo>.PrintTree(m_ignoreTree, 0);
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }

//            int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_ignoreTree, 0);
//            LOG.DebugFormat("Loaded {0} ignored files", numberOfFiles);
//        }
//        public void PrintIgnoreChildren()
//        {
//            LOG.Debug("Listing ignore files...");

//            try
//            {
//                if (m_ignoreTree.Children.Count > 0)
//                {
//                    Tree<RemoteFileInfo>.PrintTreeChildren(m_ignoreTree, RemoteRoot);
//                }
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }

//            int numberOfFiles = Tree<RemoteFileInfo>.GetNumberOfChildren(m_ignoreTree, 0);
//            numberOfFiles = (numberOfFiles == 1) ? 0 : numberOfFiles;

//            LOG.DebugFormat("Loaded {0} ignore files", numberOfFiles);
//        }

//        private void SessionFileTransferProgress(object sender, FileTransferProgressEventArgs e)
//        {
//            // Print transfer progress
//            LOG.InfoFormat("\r{0} ({1:P0} CPS = {2})", e.FileName, e.FileProgress, e.CPS);

//            // Remember a name of the last file reported
//            m_lastFileName = e.FileName;
//        }

//        public bool IgnoreAll { get; set; }
//        public Stopwatch Stopwatch { get; set; }
//        public bool TestMode { get; set; }
//        public string RemoteRoot { get; set; }
//        public string LocalRoot { get; set; }
//        public string IgnoreCache { get; set; }
//        public bool PrintTrees { get; set; }

//        public bool ReportTransferProgress
//        {
//            set
//            {
//                if (m_session != null)
//                {
//                    if (value && m_transferProgressEventCounter == 0)
//                    {
//                        m_session.FileTransferProgress += SessionFileTransferProgress;
//                        m_transferProgressEventCounter = 1;
//                    }
//                    else if (m_transferProgressEventCounter == 1)
//                    {
//                        m_session.FileTransferProgress -= SessionFileTransferProgress;
//                        m_transferProgressEventCounter = 0;
//                    }
//                }
//            }
//        }

//        private Tree<RemoteFileInfo> m_remoteTree;
//        private Tree<RemoteFileInfo> m_ignoreTree;
//        private SessionInfo m_sessionInfo;
//        private Session m_session;
//        private string m_lastFileName;
//        private List<string> m_ignoredExtensions = new List<string> { "rar", "r", "txt", "nfo", "jpg", "sfv" };
//        private int m_syncedFiles;
//        private int m_transferProgressEventCounter;

//        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//    }
//}

