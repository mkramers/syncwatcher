using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using SyncWatcherTray.Properties;
using SyncWatcherTray.ViewModel;
using Themes.Controls;

namespace SyncWatcherTray.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Style = (Style)FindResource(typeof(Window));
            MoveToBottomCorner();

            //remember last tab
            var lastTab = Settings.Default.LastSelectedTabIndex;
            TabControl.SelectedIndex = lastTab;
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

        private async void ExitApplication_OnClick(object _sender, RoutedEventArgs _e)
        {
            var viewModel = DataContext as MainViewModel;
            Debug.Assert(viewModel != null);

            if (!viewModel.CanExit())
            {
                return;
            }

            await viewModel.Dispose();

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

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        //CAPS LOCK:
        private const uint VK_CAPITAL = 0x14;
        private const uint ZKEY = 0x5A;
        private const uint EKEY = 0x45;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);

            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, EKEY))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            ShowWindow();
        }
    }
}