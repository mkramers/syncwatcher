using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Common.SFTP;
using FileGetter;
using GalaSoft.MvvmLight;
using log4net;
using WinScpApi.ViewModel;
using WinSCP;

namespace WinScpApi
{
    public class FtpManager : ViewModelBase
    {
        private readonly FtpSessionConfig m_config;
        private FtpDirectoryViewModel m_remoteRootViewModel;

        public FileHistory Cache { get; }
        public FtpClient Client { get; set; }
        public List<string> RemoteRoots { get; set; }
        public List<string> LocalRoots { get; set; }
        public string CurrentRemoteRoot { get; private set; }
        public string CurrentLocalRoot { get; set; }
        public ObservableRangeCollection<FileObject> Downloads { get; } = new ObservableRangeCollection<FileObject>();
        public bool IsDownloading => Downloads.Any();

        public FtpDirectoryViewModel RemoteRootViewModel
        {
            get => m_remoteRootViewModel;
            private set
            {
                m_remoteRootViewModel = value;
                RaisePropertyChanged();
            }
        }

        private static ILog Log { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FtpManager(FtpSessionConfig _sessionConfig, List<string> _localRoots)
        {
            Debug.Assert(_sessionConfig != null);
            Debug.Assert(_localRoots != null);

            m_config = _sessionConfig;

            List<string> remoteRoots = _sessionConfig.RemoteRoots.ToList();
            Debug.Assert(remoteRoots.Any()); //must contain roots!
            string remoteRoot = remoteRoots.First();

            Debug.Assert(_localRoots.Any()); //must contain roots!
            string localRoot = _localRoots.First();

            Cache = FileHistory.LoadOrCreate(m_config.HistoryFilePath);

            RemoteRoots = remoteRoots;
            LocalRoots = _localRoots;

            CurrentRemoteRoot = remoteRoot;
            CurrentLocalRoot = localRoot;

            //get username/password securely
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            string username = appSettings["Username"];
            string hex = ConfigurationManager.AppSettings["Password"];

            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = m_config.Host,
                UserName = username,
                Password = hex != null ? GetPasswordFromHex(hex) : "",
                SshHostKeyFingerprint = m_config.SshHostKeyFingerprint
            };
            sessionOptions.AddRawSettings("AuthGSSAPI", "1");
            sessionOptions.AddRawSettings("TcpNoDelay", "1");

            Log.Info(
                $"---\nStarting FtpManager\n===\n\tUser: {sessionOptions.UserName}\n\tHost: {sessionOptions}\n\tPath: {remoteRoot}\n");

            Client = new FtpClient(remoteRoot, sessionOptions);
            Client.ConnectionChanged += Client_ConnectionChanged;
            Client.DownloadProgress += Client_DownloadProgress;
            Client.DownloadComplete += Client_DownloadComplete;

            m_remoteRootViewModel = new FtpDirectoryViewModel(CurrentRemoteRoot, this, true);
        }

        private static string GetPasswordFromHex(string hex)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(hex));

            byte[] bytes = Enumerable.Range(0, hex.Length / 2)
                .Select(_x => Convert.ToByte(hex.Substring(_x * 2, 2), 16))
                .ToArray();
            byte[] decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            string password = Encoding.Unicode.GetString(decrypted);
            return password;
        }

        private async void Client_ConnectionChanged(object _sender, ConnectionChangedEventArgs _e)
        {
            bool isConnected = _e.IsConnected;

            if (!isConnected)
                Application.Current.Dispatcher.Invoke(() => { RemoteRootViewModel = null; });
            else
                await Refresh();

            ClientConnectionChanged?.Invoke(_sender, _e);
        }

        private void Client_DownloadComplete(object _sender, TransferEventArgs _e)
        {
            string fileName = _e.FileName;
            FtpFileViewModel matchViewModel =
                RemoteRootViewModel.Find(_file => _file.FullName == fileName) as FtpFileViewModel;

            FileObject match = matchViewModel?.File;
            if (match != null)
            {
                FileObjectState newState = !match.Cancel ? FileObjectState.COMPLETED : FileObjectState.CANCELLED;
                match.State = newState;

                Cache.AddItem(matchViewModel);
                Cache.Save(m_config.HistoryFilePath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (Downloads)
                    {
                        FileObject matchingDownload = Downloads.Find(_file => _file.FullName == fileName);
                        Debug.Assert(matchingDownload != null);

                        Downloads.Remove(matchingDownload);
                    }
                });
            }
        }

        private void Client_DownloadProgress(object _sender, FileTransferProgressEventArgs _e)
        {
            string fileName = _e.FileName;

            FileObject match = Downloads.Find(_file => _file.FullName == fileName);
            if (match != null)
            {
                double percent = _e.FileProgress * 100.0f;
                match.UpdateProgress(percent, _e.CPS);

                if (match.Cancel)
                    _e.Cancel = true;
            }
        }

        private async void WatchTimerEvent(object _sender, ElapsedEventArgs _e)
        {
            await Refresh();
        }

        public async void SetRemoteHost(string _remoteHost)
        {
            Debug.Assert(_remoteHost != null);

            CurrentRemoteRoot = _remoteHost;

            await Refresh();
        }

        public async Task Refresh()
        {
            if (!Client.IsOpened)
                return;

            OperationStarted?.Invoke(this, EventArgs.Empty);

            FtpClient client = Client;
            string root = CurrentRemoteRoot;

            Debug.Assert(client != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(root));

            Log.Info("Requesting file list...");

            //update tree
            string selectedRemoteRoot = CurrentRemoteRoot;
            FtpDirectoryViewModel remoteRootViewModel = new FtpDirectoryViewModel(selectedRemoteRoot, this, false);
            await remoteRootViewModel.LazyLoad();

            RemoteRootViewModel = remoteRootViewModel;

            OperationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public async void Sync(IEnumerable<FtpFileViewModel> _results)
        {
            Debug.Assert(_results != null);

            FtpFileViewModel[] ftpFileViewModels = _results as FtpFileViewModel[] ?? _results.ToArray();

            List<FileObject> newDownloads = ftpFileViewModels.Select(_result => _result.File).ToList();
            if (!newDownloads.Any())
                return;

            Log.Info("Starting download.");

            OperationStarted?.Invoke(this, EventArgs.Empty);

            //update file states
            foreach (FileObject file in newDownloads)
            {
                file.State = FileObjectState.PENDING;
                file.Cancel = false;
                file.UpdateProgress(0, 0);
            }

            lock (Downloads)
            {
                Downloads.AddRange(newDownloads);
            }

            await Task.Factory.StartNew(() =>
            {
                Client.DownloadFiles(newDownloads, CurrentLocalRoot, CurrentRemoteRoot, m_config);
            });

            if (Downloads.All(_download => _download.IsCompleted || _download.Cancel))
                Downloads.Clear();

            Cache.Save(m_config.HistoryFilePath);

            OperationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void CancelTransfers()
        {
            if (Client.IsOpened && Client.IsBusy)
                Client.Abort();

            foreach (FileObject pending in Downloads)
            {
                pending.Cancel = true;
                pending.State = FileObjectState.CANCELLED;
            }
        }

        public async void DeleteHistory()
        {
            Cache.Clear();
            Cache.Save(m_config.HistoryFilePath);

            if (Client.IsOpened)
                await Refresh();
        }

        public void MarkAsDownloaded(IEnumerable<FtpFileViewModel> _selected)
        {
            Debug.Assert(_selected != null);

            foreach (FtpFileViewModel item in _selected)
                Cache.AddItem(item);

            Cache.Save(m_config.HistoryFilePath);
        }

        public async Task Dispose()
        {
            if (Client.IsOpened)
                await Client.Disconnect();
            Client.Dispose();

            //cancel downloads
            CancelTransfers();

            int counter = 0;
            const int maxCounter = 10;
            while (true)
            {
                if (!Downloads.Any())
                    break;

                counter++;
                if (counter >= maxCounter)
                    break;

                await Task.Delay(200);
            }
        }

        public event EventHandler OperationStarted;
        public event EventHandler OperationCompleted;
        public event EventHandler<ConnectionChangedEventArgs> ClientConnectionChanged;
    }
}