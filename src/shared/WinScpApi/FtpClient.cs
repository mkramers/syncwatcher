using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using FileGetter;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using WinSCP;

namespace Common.SFTP
{
    public class FtpClient : ViewModelBase
    {
        private readonly SessionOptions m_options;
        private bool m_isBusy;
        private bool m_isOpened;

        private Session m_session;

        public ICommand InvertConnectionCommand
        {
            get
            {
                async void Execute()
                {
                    if (!IsOpened)
                    {
                        await Connect();
                    }
                    else
                    {
                        await Disconnect();
                    }
                }

                return new RelayCommand(Execute);
            }
        }

        public string Root { get; }

        public bool IsOpened
        {
            get => m_isOpened;
            set
            {
                if (m_isOpened != value)
                {
                    m_isOpened = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                m_isBusy = value;
                RaisePropertyChanged();
            }
        }

        public static ILog Log { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FtpClient(string _root, SessionOptions _options)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));
            Debug.Assert(_options != null);

            Root = _root;

            m_options = _options;

            m_session = new Session
            {
                SessionLogPath = "..\\..\\log.txt"
            };
        }

        private void OnFileTransferred(object _sender, TransferEventArgs _e)
        {
            DownloadComplete?.Invoke(_sender, _e);
        }

        private void OnFileTransferProgress(object _sender, FileTransferProgressEventArgs _e)
        {
            DownloadProgress?.Invoke(_sender, _e);
        }

        public async Task Connect()
        {
            if (m_session.Opened)
            {
                return;
            }

            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            await Task.Factory.StartNew(
                () =>
                {
                    Log.Info("Connecting to remote host...");
                    try
                    {
                        m_session.Open(m_options);
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"Error connecting to {m_options.HostName}:\n\n{e.Message}");
                    }
                });

            IsBusy = false;

            string message = m_session.Opened ? "Connect opened successfully" : "Connection failed to open";
            UpdateState(m_session.Opened, message);
        }

        public async Task Disconnect()
        {
            if (!m_session.Opened)
            {
                return;
            }

            if (IsBusy)
            {
                return;
            }

            await Task.Factory.StartNew(
                () =>
                {
                    Log.Info("Disconnecting from remote host...");
                    m_session.Close();
                });

            const string message = "Disconnected";
            UpdateState(m_session.Opened, message);
        }

        public void Abort()
        {
            if (m_session.Opened)
            {
                m_session.Abort();

                m_session = new Session
                {
                    SessionLogPath = "..\\..\\log.txt"
                };
            }
        }

        public async Task<RemoteDirectoryInfo> GetDirectories(string _directory)
        {
            Debug.Assert(_directory != null);

            if (!m_session.Opened)
            {
                return null;
            }

            IsBusy = true;

            RemoteDirectoryInfo directories = await Task.Factory.StartNew(
                () =>
                {
                    RemoteDirectoryInfo remoteDirectoryInfo = null;
                    try
                    {
                        remoteDirectoryInfo = m_session.ListDirectory(_directory);
                    }
                    catch (Exception e)
                    {
                        string message = $"Error!\n{e.Message}";
                        UpdateState(false, message);
                    }

                    return remoteDirectoryInfo;
                });

            IsBusy = false;

            return directories;
        }

        public async Task<List<FileObject>> TryListFiles(string _path)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_path));

            if (IsBusy)
            {
                return null;
            }
            IsBusy = true;

            List<FileObject> files = await Task.Factory.StartNew(
                () =>
                {
                    List<FileObject> fileList = null;
                    try
                    {
                        if (m_session.Opened)
                        {
                            fileList = ListFiles(m_session, _path);
                        }
                    }
                    catch (Exception e)
                    {
                        string message = $"Error!\n{e.Message}";
                        UpdateState(false, message);
                    }
                    return fileList;
                });

            IsBusy = false;

            return files;
        }

        private List<FileObject> ListFiles(Session _session, string _path)
        {
            Debug.Assert(_session != null);
            Debug.Assert(_session.Opened);
            Debug.Assert(!string.IsNullOrWhiteSpace(_path));

            List<FileObject> files = new List<FileObject>();

            if (TryListDirectory(_session, _path, out RemoteDirectoryInfo remoteDirectory))
            {
                List<RemoteFileInfo> items = remoteDirectory.Files.ToList();

                foreach (RemoteFileInfo item in items)
                {
                    if (item.Name == "." || item.Name == "..")
                    {
                        continue;
                    }

                    if (item.IsDirectory)
                    {
                        List<FileObject> containedFiles = ListFiles(_session, item.FullName);
                        files.AddRange(containedFiles);
                    }
                    else
                    {
                        FileObject file = new FileObject(item, _path);
                        files.Add(file);
                    }
                }
            }

            return files;
        }

        private bool TryListDirectory(Session _session, string _path, out RemoteDirectoryInfo _remoteDirectory)
        {
            bool success = false;

            _remoteDirectory = null;
            //try
            {
                _remoteDirectory = _session.ListDirectory(_path);
                success = true;
            }
            //catch (SessionLocalException e)
            //{
            //    Console.WriteLine(e);
            //    throw e;
            //}

            return success;
        }

        public bool DownloadFiles(List<FileObject> _files, string _destinationRoot, string _remoteRoot, SessionConfig _sessionConfig)
        {
            Debug.Assert(_files != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_destinationRoot));
            Debug.Assert(!string.IsNullOrWhiteSpace(_remoteRoot));
            Debug.Assert(_sessionConfig != null);

            bool success = false;
            SessionOptions sessionOptions = m_options;

            try
            {
                string localPath = _destinationRoot;
                string remotePath = _remoteRoot;

                DateTime started = DateTime.Now;
                int count = 0;
                long bytes = 0;

                Console.WriteLine("Starting files enumeration...");
                IEnumerator<FileObject> filesEnumerator = _files.GetEnumerator();

                List<Task> tasks = new List<Task>();

                for (int i = 0; i < _sessionConfig.NumberOfSlots; i++)
                {
                    int displayIndex = i + 1;
                    Task task = new Task(
                        () =>
                        {
                            using (Session downloadSession = new Session())
                            {
                                Console.WriteLine("Starting download {0}...", displayIndex);
                                downloadSession.FileTransferProgress += OnFileTransferProgress;
                                downloadSession.FileTransferred += OnFileTransferred;
                                downloadSession.Open(sessionOptions);

                                while (true)
                                {
                                    string remoteFilePath;
                                    lock (filesEnumerator)
                                    {
                                        if (!filesEnumerator.MoveNext())
                                        {
                                            break;
                                        }

                                        FileObject file = filesEnumerator.Current;
                                        Debug.Assert(file != null);
                                        if (file.Cancel)
                                        {
                                            continue;
                                        }

                                        file.State = FileObjectState.IN_PROGRESS;
                                        Debug.Assert(file != null);
                                        bytes += file.Size;
                                        count++;
                                        remoteFilePath = file.FullName;
                                    }

                                    string localFilePath = downloadSession.TranslateRemotePathToLocal(remoteFilePath, remotePath, localPath);
                                    Console.WriteLine("Downloading {0} to {1} in {2}...", remoteFilePath, localFilePath, displayIndex);

                                    string localDirectory = Path.GetDirectoryName(localFilePath);
                                    Debug.Assert(localDirectory != null);
                                    if (!Directory.Exists(localDirectory))
                                    {
                                        Directory.CreateDirectory(localDirectory);
                                    }

                                    bool deleteSource = _sessionConfig.DeleteSource;
                                    downloadSession.GetFiles(downloadSession.EscapeFileMask(remoteFilePath), localFilePath, deleteSource).Check();
                                }

                                Console.WriteLine("Download {0} done", displayIndex);
                            }
                        });

                    tasks.Add(task);
                    task.Start();
                }

                Console.WriteLine("Waiting for downloads to complete...");
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("Done");

                DateTime ended = DateTime.Now;
                Console.WriteLine("Took {0}", ended - started);
                Console.WriteLine("Downloaded {0} files, totaling {1:N0} bytes", count, bytes);

                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }

            return success;
        }

        private void UpdateState(bool _isOpened, string _message)
        {
            Debug.Assert(_message != null);

            IsOpened = _isOpened;

            Log.Info(_message);

            ConnectionChangedEventArgs args = new ConnectionChangedEventArgs(_isOpened, _message);
            ConnectionChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            m_session.Dispose();
        }

        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        public event FileTransferProgressEventHandler DownloadProgress;
        public event FileTransferredEventHandler DownloadComplete;
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public string Message { get; }

        public ConnectionChangedEventArgs(bool _isConnected, string _message)
        {
            Debug.Assert(_message != null);

            IsConnected = _isConnected;
            Message = _message;
        }
    }
}
