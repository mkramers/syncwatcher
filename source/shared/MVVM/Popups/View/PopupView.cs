using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MVVM.Popups.View
{
    public class PopupView : UserControl
    {
        public PopupView()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object _sender, RoutedEventArgs _e)
        {
            Focusable = true;
            Keyboard.Focus(this);
        }
    }
}