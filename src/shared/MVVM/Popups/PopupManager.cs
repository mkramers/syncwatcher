using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MVVM.Popups.View;
using MVVM.Popups.ViewModel;

namespace MVVM.Popups
{
    public partial class PopupManager
    {
        public void ShowInfo(string _message, string _windowTitle)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_message));
            Debug.Assert(_windowTitle != null);

            InfoPopupViewModel info = new InfoPopupViewModel(_message);
            Show(info, PopupBehavior.BLOCKING, ResizeMode.NoResize, _windowTitle);
        }

        public void ShowError(string _message, string _windowTitle)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_message));
            Debug.Assert(_windowTitle != null);

            ErrorPopupViewModel error = new ErrorPopupViewModel(_message);
            Show(error, PopupBehavior.BLOCKING, ResizeMode.NoResize, _windowTitle);
        }
    }

    public partial class PopupManager
    {
        public static PopupManager Instance { get; } = new PopupManager();

        public List<Popup> Popups { get; } = new List<Popup>();

        public void Exit()
        {
            //make a copy so can modify the popup list while we iterate
            List<Popup> popups = new List<Popup>(Popups);
            foreach (Popup popup in popups)
            {
                PopupWindow window = popup.Window;
                Debug.Assert(window != null);

                window.Close();
            }
            popups.Clear();
        }

        public void ClosePopup(CloseableViewModel _viewModel)
        {
            Close(_viewModel);
        }

        public void Show(IPopupViewModel _viewModel, PopupBehavior _behavior, ResizeMode _resizeMode, string _windowTitle, Size? _size = null)
        {
            Debug.Assert(_viewModel != null);

            switch (_behavior)
            {
                case PopupBehavior.BLOCKING:
                    ShowNewWindow(_viewModel, _behavior, _resizeMode, _windowTitle, _size);
                    break;
                case PopupBehavior.SINGLE:
                case PopupBehavior.SINGLE_OFTYPE:
                {
                    List<Popup> existing;

                    switch (_behavior)
                    {
                        case PopupBehavior.SINGLE:
                            existing = Find(_viewModel);
                            break;
                        case PopupBehavior.SINGLE_OFTYPE:
                            existing = FindSameType(_viewModel);
                            break;
                        case PopupBehavior.SINGLE_NEW:
                        case PopupBehavior.MULTIPLE:
                        case PopupBehavior.BLOCKING:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_behavior), _behavior, null);
                    }

                    if (existing.Any())
                    {
                        Popup first = existing.First();
                        Debug.Assert(first.Behavior == _behavior);

                        Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                PopupWindow window = first.Window;
                                if (window.Visibility != Visibility.Visible)
                                {
                                    window.Visibility = Visibility.Visible;
                                }

                                ResizeWindow(window, _size);

                                window.Activate();
                            });
                    }
                    else
                    {
                        ShowNewWindow(_viewModel, _behavior, _resizeMode, _windowTitle, _size);
                    }
                }
                    break;
                case PopupBehavior.SINGLE_NEW:
                {
                    List<Popup> existing = Find(_viewModel);
                    foreach (Popup popup in existing)
                    {
                        Remove(popup);
                    }

                    ShowNewWindow(_viewModel, _behavior, _resizeMode, _windowTitle, _size);
                }
                    break;
                case PopupBehavior.MULTIPLE:
                    ShowNewWindow(_viewModel, _behavior, _resizeMode, _windowTitle, _size);
                    break;
            }
        }

        private void ShowNewWindow(IPopupViewModel _viewModel, PopupBehavior _behavior, ResizeMode _resizeMode, string _windowTitle, Size? _size = null)
        {
            Debug.Assert(_viewModel != null);

            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Window mainWindow = Application.Current.MainWindow;

                    PopupWindowViewModel popupViewModel = new PopupWindowViewModel(_viewModel)
                    {
                        Title = _windowTitle
                    };

                    PopupWindow window = new PopupWindow
                    {
                        DataContext = popupViewModel,
                        ResizeMode = _resizeMode,
                        Owner = mainWindow
                    };

                    ResizeWindow(window, _size);

                    window.Closed += Window_Closed;
                    window.Closing += _viewModel.OnWindowClosing;

                    _viewModel.OnRequestClose += (_sender, _e) => window.Close();

                    Popup popup = new Popup(window, popupViewModel, _behavior);
                    Popups.Add(popup);

                    if (_behavior == PopupBehavior.BLOCKING)
                    {
                        window.ShowDialog();
                    }
                    else
                    {
                        window.Show();
                    }
                });
        }

        private static void ResizeWindow(Window _window, Size? _size)
        {
            Debug.Assert(_window != null);

            if (_size != null)
            {
                Size size = _size.Value;
                Debug.Assert(size.Width > 0);
                Debug.Assert(size.Height > 0);

                _window.Width = size.Width;
                _window.Height = size.Height;
                _window.SizeToContent = SizeToContent.Manual;
            }
        }

        public List<Popup> FindofType(Type _type)
        {
            List<Popup> existing = new List<Popup>();
            foreach (Popup popup in Popups)
            {
                PopupWindowViewModel popupWindowViewModel = popup.ViewModel as PopupWindowViewModel;
                Debug.Assert(popupWindowViewModel != null);

                IPopupViewModel viewModel = popupWindowViewModel.ViewModel;

                if (viewModel.GetType() == _type)
                {
                    existing.Add(popup);
                }
            }
            return existing;
        }

        public void Hide(IPopupViewModel _viewModel)
        {
            if (_viewModel != null)
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        List<Popup> existing = Find(_viewModel);
                        if (existing.Any())
                        {
                            Popup first = existing.First();

                            PopupWindow window = first.Window;
                            window.Hide();
                        }
                    });
            }
        }

        public void Close(IPopupViewModel _viewModel)
        {
            if (_viewModel != null)
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        List<Popup> existing = Find(_viewModel);
                        if (existing.Any())
                        {
                            Popup first = existing.First();

                            PopupWindow window = first.Window;
                            window.Close();
                        }
                    });
            }
        }

        private void Window_Closed(object _sender, EventArgs _e)
        {
            Window window = _sender as Window;
            Debug.Assert(window != null);

            PopupWindowViewModel viewModel = window.DataContext as PopupWindowViewModel;
            Debug.Assert(viewModel != null);

            List<Popup> popups = Find(viewModel.ViewModel);
            foreach (Popup popup in popups)
            {
                //ensure they are equal!
                PopupWindow popupWindow = popup.Window;
                Debug.Assert(Equals(popupWindow, window));

                Remove(popup);
            }
        }

        private void Remove(Popup _popup)
        {
            Debug.Assert(_popup != null);

            OnPopupClosed(_popup);

            bool removed = Popups.Remove(_popup);
            Debug.Assert(removed);
        }

        public void Activate(IPopupViewModel _viewModel)
        {
            Debug.Assert(_viewModel != null);

            List<Popup> popups = Find(_viewModel);

            foreach (Popup popup in popups)
            {
                PopupWindow window = popup.Window;

                Application.Current.Dispatcher.Invoke(() => { window.Activate(); });
            }
        }

        public List<Popup> Find(IPopupViewModel _viewModel)
        {
            Debug.Assert(_viewModel != null);

            List<Popup> popups = new List<Popup>();
            foreach (Popup popup in Popups)
            {
                PopupWindowViewModel windowViewModel = popup.ViewModel as PopupWindowViewModel;
                Debug.Assert(windowViewModel != null);

                IPopupViewModel viewModel = windowViewModel.ViewModel;
                if (Equals(viewModel, _viewModel))
                {
                    popups.Add(popup);
                }
            }
            return popups;
        }

        public List<Popup> FindSameType(IPopupViewModel _viewModel)
        {
            Debug.Assert(_viewModel != null);

            List<Popup> popups = new List<Popup>();
            foreach (Popup popup in Popups)
            {
                PopupWindowViewModel windowViewModel = popup.ViewModel as PopupWindowViewModel;
                Debug.Assert(windowViewModel != null);
                IPopupViewModel viewModel = windowViewModel.ViewModel;
                if (viewModel.GetType() == _viewModel.GetType())
                {
                    popups.Add(popup);
                }
            }
            return popups;
        }

        private void OnPopupClosed(Popup _popup)
        {
            PopupClosed?.Invoke(_popup, EventArgs.Empty);
        }

        public event EventHandler PopupClosed;
    }
}
