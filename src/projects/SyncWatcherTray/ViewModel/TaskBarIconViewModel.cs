using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Common.Mvvm;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace SyncWatcherTray.ViewModel
{
    public class TaskbarIconViewModel : ViewModelBase
    {
        public TaskbarIconViewModel()
        {
            CreateIcons();

            UpdateIcon(m_idleIcon);

            //start icon timer
            m_timer = new Timer(ICON_INTERVAL);
            m_timer.Elapsed += Timer_Elapsed;
        }

        public void SetIsBusy()
        {
            Debug.Assert(!m_isBusy);

            UpdateIcon(m_green);

            m_timer.Start();

            m_isBusy = false;
        }

        public void SetIsNotBusy()
        {
            Debug.Assert(m_isBusy);

            m_timer.Stop();

            UpdateIcon(m_idleIcon);

            m_isBusy = false;
        }

        private void CreateIcons()
        {
            var darkUri = new Uri("pack://application:,,,/Resources/coffeeDark.ico");
            m_idleIcon = new BitmapImage(darkUri);
            m_idleIcon.Freeze();

            var greenUri = new Uri("pack://application:,,,/Resources/coffeeGreen.ico");
            m_green = new BitmapImage(greenUri);
            m_green.Freeze();

            var greenBusyUri = new Uri("pack://application:,,,/Resources/coffeeGreenBusy.ico");
            m_greenBusy = new BitmapImage(greenBusyUri);
            m_greenBusy.Freeze();
        }/

        private void Timer_Elapsed(object _sender, ElapsedEventArgs _e)
        {
            if (m_isBusy)
            {
                var uri = m_iconAnimateState ? m_green : m_greenBusy;

                UpdateIcon(uri);

                m_iconAnimateState = !m_iconAnimateState;
            }
        }

        private void UpdateIcon(ImageSource _image)
        {
            Debug.Assert(_image != null);

            Dispatcher.CurrentDispatcher.Invoke(() => IconSource = _image);
        }

        public ImageSource IconSource
        {
            get => m_iconSource;
            set
            {
                m_iconSource = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage m_idleIcon;
        private BitmapImage m_green;
        private BitmapImage m_greenBusy;
        private bool m_iconAnimateState;
        private ImageSource m_iconSource;

        private bool m_isBusy;
        private readonly Timer m_timer;

        private const int ICON_INTERVAL = 222;
    }
}