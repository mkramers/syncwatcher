using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileLister.ViewModel;

namespace FileLister.View
{
    public partial class MultiFileViewerView : UserControl
    {
        public MultiFileViewerView()
        {
            InitializeComponent();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.R:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        ShowRenamerWindow();
                    break;
                case Key.S:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        ShowSettingsWindow();
                    break;
                default:
                    break;
            }
        }

        private void OnClick_ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }

        private void ShowSettingsWindow()
        {
            if (!IsSettingsViewShown)
            {
                var settingsView = new SettingsWindow {Topmost = true};
                settingsView.Closed += SettingsWindowClosed;
                settingsView.Show();

                IsSettingsViewShown = true;
            }
        }

        private void OnClick_ShowRenamerWindow(object sender, RoutedEventArgs e)
        {
            ShowRenamerWindow();
        }

        private void ShowRenamerWindow()
        {
            if (!IsRenamerViewShown)
            {
                var renamerView = new RenamerView
                {
                    DataContext = ViewModel.RenamerViewModel,
                    Topmost = true
                };
                renamerView.Closed += RenamerWindowClosed;
                renamerView.Show();

                IsRenamerViewShown = true;
            }
        }

        private void SettingsWindowClosed(object sender, EventArgs e)
        {
            IsSettingsViewShown = false;
        }

        private void RenamerWindowClosed(object sender, EventArgs e)
        {
            IsRenamerViewShown = false;
        }

        public MultiFileViewerViewModel ViewModel => (MultiFileViewerViewModel) DataContext;

        public bool IsSettingsViewShown { get; private set; }
        public bool IsRenamerViewShown { get; private set; }
    }
}