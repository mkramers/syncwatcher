using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Common.Win32
{
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