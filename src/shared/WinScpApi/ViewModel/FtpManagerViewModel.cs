using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.Framework.EventHelpers;
using Common.SFTP;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace WinScpApi.ViewModel
{
    public class FtpManagerViewModel : ViewModelBase
    {
        private string m_disconnectedMessage;
        private IList m_selectedDownloads;

        public ICommand SyncSelectedCommand
        {
            get
            {
                async void Execute()
                {
                    FtpDirectoryViewModel remoteDirectoryViewModel = Manager.RemoteRootViewModel;

                    IEnumerable<FtpFileViewModel> selectedFiles = await remoteDirectoryViewModel.GetSelectedFiles();
                    if (selectedFiles != null)
                    {
                        Manager.Sync(selectedFiles);
                    }
                }

                bool CanExecute()
                {
                    return Manager != null;
                }

                return new RelayCommand(Execute, CanExecute);
            }
        }
        public ICommand RefreshCommand
        {
            get
            {
                async void Execute()
                {
                    await Manager.Refresh();
                }

                bool CanExecute()
                {
                    return Manager.Client.IsOpened;
                }

                return new RelayCommand(Execute, CanExecute);
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                void Execute()
                {
                    Manager.CancelTransfers();
                }

                bool CanExecute()
                {
                    return Manager != null;
                }

                return new RelayCommand(Execute, CanExecute);
            }
        }
        public ICommand DeleteHistoryCommand
        {
            get
            {
                void Execute()
                {
                    Manager.DeleteHistory();
                }

                bool CanExecute()
                {
                    return Manager != null;
                }

                return new RelayCommand(Execute, CanExecute);
            }
        }

        public FtpManager Manager { get; }
        public IList SelectedDownloads
        {
            get => m_selectedDownloads;
            set
            {
                m_selectedDownloads = value;
                RaisePropertyChanged();
            }
        }
        public string SelectedRemoteRoot
        {
            get => Manager.CurrentRemoteRoot;
            set => Manager.SetRemoteHost(value);
        }
        public string SelectedLocalRoot
        {
            get => Manager.CurrentLocalRoot;
            set
            {
                if (Manager.CurrentLocalRoot != value)
                {
                    Manager.CurrentLocalRoot = value;
                    RaisePropertyChanged();

                    LocalRootChanged?.Invoke(this, new StringEventArgs(value));
                }
            }
        }
        public string DisconnectedMessage
        {
            get => m_disconnectedMessage;
            set
            {
                m_disconnectedMessage = value;
                RaisePropertyChanged();
            }
        }

        public FtpManagerViewModel(FtpManager _manager)
        {
            Debug.Assert(_manager != null);

            Manager = _manager;
            Manager.ClientConnectionChanged += Manager_OnClientConnectionChanged;
        }

        public event EventHandler<StringEventArgs> LocalRootChanged;

        public bool CanExit()
        {
            bool canExit = !Manager.Client.IsBusy;
            return canExit;
        }

        public async Task Dispose()
        {
            await Manager.Dispose();
        }

        private void Manager_OnClientConnectionChanged(object _sender, ConnectionChangedEventArgs _e)
        {
            string message = _e.Message;

            DisconnectedMessage = message;
        }
    }
}
