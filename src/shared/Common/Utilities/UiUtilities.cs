using System.Diagnostics;
using System.Windows;

namespace Common
{
    public static class UiUtilities
    {
        public static void BringToFront(Window _window)
        {
            Debug.Assert(_window != null);

            if (!_window.IsVisible)
                _window.Show();

            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;
        }
    }
}