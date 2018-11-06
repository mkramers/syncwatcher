using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Common.Win32;
using MVVM.View;
using SyncWatcherTray.Properties;
using SyncWatcherTray.ViewModel;
using Themes.Controls;

namespace SyncWatcherTray.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDisposable
    {
        private readonly SafeNativeMethods m_safeNativeMethods;
        private bool m_isShown;

        public MainWindow()
        {
            InitializeComponent();

            Activated += MainWindow_Activated;

            Style = (Style) FindResource(typeof(Window));
            MoveToBottomCorner();

            m_safeNativeMethods = new SafeNativeMethods();
            m_safeNativeMethods.HotKeyPressed += SafeNativeMethods_OnHotKeyPressed;
        }

        //global hotkey

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void MainWindow_Activated(object _sender, EventArgs _e)
        {
            UserControl userControl = TabControl.SelectedContent as UserControl;
            Debug.Assert(userControl != null);

            if (userControl is ISearchableView searchableView)
            {
                searchableView.Activate();
            }
        }

        private void SafeNativeMethods_OnHotKeyPressed(object _sender, EventArgs _e)
        {
            if (m_isShown)
            {
                HideWindow();
            }
            else
            {
                ShowWindow();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            m_safeNativeMethods.Initialize(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            m_safeNativeMethods.Stop(this);

            base.OnClosed(e);
        }

        private void MoveToBottomCorner()
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;

            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void OnClickMinimize(object _sender, RoutedEventArgs _e)
        {
            HideWindow();
        }

        private void OnShowWindow(object _sender, RoutedEventArgs _e)
        {
            ShowWindow();
        }

        private void OnHideWindow(object _sender, RoutedEventArgs _e)
        {
            HideWindow();
        }

        private void HideWindow()
        {
            if (m_isShown)
            {
                Window mainWindow = Application.Current.MainWindow;
                mainWindow?.Hide();

                m_isShown = false;
            }
        }

        public void ShowWindow()
        {
            if (!(Application.Current.MainWindow is MainWindow currentWindow))
            {
                currentWindow = this;

                Application.Current.MainWindow = currentWindow;
            }

            if (!m_isShown)
            {
                currentWindow.Show();
            }

            m_isShown = true;
        }

        private void ExitApplication_OnClick(object _sender, RoutedEventArgs _e)
        {
            MainViewModel viewModel = DataContext as MainViewModel;
            Debug.Assert(viewModel != null);

            if (!viewModel.CanExit())
            {
                return;
            }

            Dispose();

            Application.Current.Shutdown();
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object _sender, RoutedEventArgs _e)
        {
            bool isWindowShown = m_isShown;
            if (!isWindowShown)
            {
                OnShowWindow(_sender, _e);
            }
            else
            {
                OnHideWindow(_sender, _e);
            }
        }

        private void Dispose(bool _isDisposing)
        {
            if (_isDisposing)
            {
                MainViewModel viewModel = DataContext as MainViewModel;
                Debug.Assert(viewModel != null);

                viewModel.Dispose();

                m_safeNativeMethods.Dispose();
            }
        }
    }
}
