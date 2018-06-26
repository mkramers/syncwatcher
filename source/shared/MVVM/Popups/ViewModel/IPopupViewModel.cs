using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace MVVM.Popups.ViewModel
{
    public interface IPopupViewModel : ICleanup
    {
        void OnWindowClosing(object _sender, CancelEventArgs _e);

        event EventHandler OnRequestClose;
    }
}