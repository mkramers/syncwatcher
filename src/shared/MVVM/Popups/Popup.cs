using GalaSoft.MvvmLight;
using MVVM.Popups.View;

namespace MVVM.Popups
{
    public class Popup
    {
        public PopupWindow Window { get; set; }
        public ViewModelBase ViewModel { get; set; }
        public PopupBehavior Behavior { get; set; }

        public Popup(PopupWindow _window, ViewModelBase _viewModel, PopupBehavior _behavior)
        {
            Window = _window;
            ViewModel = _viewModel;
            Behavior = _behavior;
        }
    }
}
