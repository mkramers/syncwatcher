using System.Diagnostics;
using System.Windows.Controls;
using SyncWatcherTray.Properties;
using SyncWatcherTray.ViewModel;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace SyncWatcherTray.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void PropertyGrid_OnPropertyValueChanged(object _sender, PropertyValueChangedEventArgs _e)
        {
            Settings settings = DataContext as Settings;
            Debug.Assert(settings != null);

            settings.Save();
        }
    }
}
