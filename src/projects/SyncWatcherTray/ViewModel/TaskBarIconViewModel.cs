using System;
using System.Diagnostics;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;

namespace SyncWatcherTray.ViewModel
{
    public class TaskbarIconViewModel : ViewModelBase
    {
        private const int ICON_INTERVAL = 222;

        private readonly Timer m_timer;
        private BitmapImage m_green;
        private BitmapImage m_greenBusy;
        private bool m_iconAnimateState;
        private ImageSource m_iconSource;
        private BitmapImage m_idleIcon;

        public ImageSource IconSource
        {
            get => m_iconSource;
            set
            {
                m_iconSource = value;
                RaisePropertyChanged();
            }
        }
        public bool IsBusy { get; private set; }

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
            Debug.Assert(!IsBusy);

            UpdateIcon(m_green);

            m_timer.Start();

            IsBusy = true;
        }

        public void SetIsNotBusy()
        {
            Debug.Assert(IsBusy);

            m_timer.Stop();

            UpdateIcon(m_idleIcon);

            IsBusy = false;
        }

        private void CreateIcons()
        {
            Uri darkUri = new Uri("pack://application:,,,/Resources/coffeeDark.ico");
            m_idleIcon = new BitmapImage(darkUri);
            m_idleIcon.Freeze();

            Uri greenUri = new Uri("pack://application:,,,/Resources/coffeeGreen.ico");
            m_green = new BitmapImage(greenUri);
            m_green.Freeze();

            Uri greenBusyUri = new Uri("pack://application:,,,/Resources/coffeeGreenBusy.ico");
            m_greenBusy = new BitmapImage(greenBusyUri);
            m_greenBusy.Freeze();
        }

        private void Timer_Elapsed(object _sender, ElapsedEventArgs _e)
        {
            if (IsBusy)
            {
                BitmapImage uri = m_iconAnimateState ? m_green : m_greenBusy;

                UpdateIcon(uri);

                m_iconAnimateState = !m_iconAnimateState;
            }
        }

        private void UpdateIcon(ImageSource _image)
        {
            Debug.Assert(_image != null);

            Dispatcher.CurrentDispatcher.Invoke(() => IconSource = _image);
        }
    }
}