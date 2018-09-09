using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Common.Win32;
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

        public MainWindow()
        {
            InitializeComponent();

            Style = (Style)FindResource(typeof(Window));
            MoveToBottomCorner();

            //remember last tab
            var lastTab = Settings.Default.LastSelectedTabIndex;
            TabControl.SelectedIndex = lastTab;

            m_safeNativeMethods = new SafeNativeMethods();
            m_safeNativeMethods.HotKeyPressed += SafeNativeMethods_OnHotKeyPressed;
        }

        private void SafeNativeMethods_OnHotKeyPressed(object _sender, EventArgs _e)
        {
            ShowWindow();
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
            var desktopWorkingArea = SystemParameters.WorkArea;

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
                var mainWindow = Application.Current.MainWindow;
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

            currentWindow.Activate();
            currentWindow.Focus();
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
            var isWindowShown = m_isShown;
            if (!isWindowShown)
            {
                OnShowWindow(_sender, _e);
            }
            else
            {
                OnHideWindow(_sender, _e);
            }
        }

        private void TabControl_OnSelectionChanged(object _sender, SelectionChangedEventArgs _e)
        {
            var tabControl = _sender as TabControlEx;
            Debug.Assert(tabControl != null);

            var index = tabControl.SelectedIndex;

            Settings.Default.LastSelectedTabIndex = index;
            Settings.Default.Save();
        }

        private bool m_isShown;

        //global hotkey

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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