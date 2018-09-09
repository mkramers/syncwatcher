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

    public class SafeNativeMethods : IDisposable
    {
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

        public event EventHandler<EventArgs> HotKeyPressed;

        private void RegisterHotKey(Window _window)
        {
            Debug.Assert(_window != null);

            WindowInteropHelper helper = new WindowInteropHelper(_window);

            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, EKEY))
            {
                // handle error
            }
        }

        private void UnregisterHotKey(Window _window)
        {
            Debug.Assert(_window != null);

            WindowInteropHelper helper = new WindowInteropHelper(_window);
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
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void Stop(Window _window)
        {
            Debug.Assert(_window != null);

            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
            }

            UnregisterHotKey(_window);
        }

        public void Initialize(Window _window)
        {
            Debug.Assert(_window != null);

            WindowInteropHelper helper = new WindowInteropHelper(_window);

            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_window);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                _source?.Dispose();
            }
        }
    }
}