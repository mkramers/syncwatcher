using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.Framework.EventHelpers;
using Common.Mvvm;
using Common.SFTP;
using GalaSoft.MvvmLight;
using MVVM.Popups;

namespace WinScpApi.ViewModel
{
    public class FtpManagerViewModel : ViewModelBase
    {
        public event EventHandler<StringEventArgs> LocalRootChanged;

        public FtpManagerViewModel(FtpManager _manager)
        {
            Debug.Assert(_manager != null);

            Manager = _manager;
            Manager.ClientConnectionChanged += Manager_OnClientConnectionChanged;
        }

        public bool CanExit()
        {
            var canExit = !Manager.Client.IsBusy;
            return canExit;
        }

        public async Task Dispose()
        {
            await Manager.Dispose();
        }

        private void Manager_OnClientConnectionChanged(object _sender, ConnectionChangedEventArgs _e)
        {
            var message = _e.Message;

            DisconnectedMessage = message;
        }

        public ICommand SyncSelectedCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        var remoteDirectoryViewModel = Manager.RemoteRootViewModel;

                        var selectedFiles = await remoteDirectoryViewModel.GetSelectedFiles();
                        if (selectedFiles != null)
                            Manager.Sync(selectedFiles);
                    },
                    CanExecuteFunc = () => Manager != null
                };
            }
        }

        public ICommand SyncAllCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => throw new NotImplementedException(),
                    CanExecuteFunc = () => false
                };
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () => await Manager.Refresh(),
                    CanExecuteFunc = () => Manager.Client.IsOpened
                };
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Manager.CancelTransfers(),
                    CanExecuteFunc = () => Manager != null
                };
            }
        }

        public ICommand DeleteHistoryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Manager.DeleteHistory(),
                    CanExecuteFunc = () => Manager != null
                };
            }
        }

        public ICommand TestCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        var text =
                            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed mi neque, accumsan vel magna ut, elementum molestie ligula. Mauris dignissim gravida ornare. Sed at laoreet turpis. Sed sodales vel felis et congue. Maecenas feugiat odio nunc, eu viverra ante interdum sit amet. Proin fringilla elit a libero pellentesque, varius lacinia arcu efficitur. Aliquam venenatis sagittis imperdiet. Donec ac eros sit amet magna semper rhoncus eu ac justo. Integer rhoncus felis ut ipsum porttitor laoreet. Nam nulla orci, auctor sit amet sem eu, vestibulum dictum arcu. Proin porttitor orci et felis sollicitudin mollis. Etiam ac libero ac lacus tincidunt sollicitudin. Aenean eu libero ultricies, molestie dolor at, varius libero. Ut ullamcorper tellus a lacus blandit blandit. Ut quis mi at nisl aliquam lacinia.

Praesent sagittis metus vitae iaculis luctus. Etiam pretium nunc dui, ut fringilla turpis vestibulum fringilla. Integer viverra magna leo, ac aliquam sapien interdum eget. Suspendisse potenti. Vivamus aliquam consequat interdum. Aliquam tincidunt est nec orci varius, et pellentesque odio convallis. Integer tempor velit non ex imperdiet tristique. Duis risus lectus, consectetur rhoncus velit nec, aliquam iaculis risus. In consectetur arcu sed neque aliquam, at scelerisque sem dapibus. Nulla at lacus tristique, malesuada dolor ut, cursus risus. Nulla placerat tortor eget purus euismod vehicula. Integer gravida sollicitudin aliquam. Pellentesque pretium venenatis suscipit. Sed vel ex quis eros viverra posuere luctus et turpis. Donec suscipit dictum neque, et mattis ipsum malesuada nec. Mauris pharetra convallis sapien, ac maximus augue mollis eu.

Nam a tempus sapien, eu ornare magna. In maximus, libero in luctus rhoncus, eros lectus aliquam felis, et pulvinar nulla elit sit amet nibh. Morbi eu fringilla metus. Donec eget mattis ligula, in faucibus lectus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ornare lectus quis blandit lobortis. Cras a ante dolor. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Mauris eu nunc ut nibh condimentum vestibulum.

Suspendisse eu ex et lacus maximus ullamcorper vitae vitae urna. Nunc ut sodales eros. Ut lacinia id orci non dignissim. Fusce lacinia lectus sit amet erat tempor scelerisque. Donec gravida nunc dignissim, malesuada nunc aliquam, ultricies massa. Vivamus vehicula purus eu luctus mattis. Pellentesque vulputate massa nec urna fermentum tristique.

Quisque vel nisi porta, porta mi vitae, vulputate augue. Sed ac vehicula nibh. Sed vel scelerisque nunc. Proin id erat eu tellus tempus euismod eget ullamcorper nisl. Nam posuere odio vel turpis aliquam ultricies. Curabitur quis leo maximus, condimentum risus a, tempor justo. Ut eros lacus, aliquet vel porttitor id, facilisis non risus. In eget odio vitae erat vulputate euismod.";

                        PopupManager.Instance.ShowError(text, "Titlesssssssss");
                    },
                    CanExecuteFunc = () => Manager != null
                };
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

        private string m_disconnectedMessage;

        private IList m_selectedDownloads;
    }
}