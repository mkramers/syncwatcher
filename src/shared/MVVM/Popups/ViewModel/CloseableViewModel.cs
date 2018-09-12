using System;
using System.ComponentModel;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace MVVM.Popups.ViewModel
{
    public class CloseableViewModel : ViewModelBase, IPopupViewModel
    {
        public RelayCommand RequestCloseCommand => new RelayCommand(RequestClose);

        public DialogResult Result { get; private set; }

        public void OnWindowClosing(object _sender, CancelEventArgs _e)
        {
            if (Result == DialogResult.None)
            {
                Result = DialogResult.Cancel;
            }

            OnClosing(_sender, _e);
        }

        public event EventHandler OnRequestClose;

        protected virtual void OnClosing(object _sender, CancelEventArgs _e)
        {
        }

        public void RequestClose(DialogResult _result)
        {
            Result = _result;
            RequestClose();
        }

        public void RequestClose()
        {
            OnRequestClose?.Invoke(this, new EventArgs());
        }
    }
}
