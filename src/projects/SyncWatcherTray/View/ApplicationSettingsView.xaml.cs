using System.Diagnostics;
using System.Windows.Controls;
using Common.SFTP;
using SyncWatcherTray.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace SyncWatcherTray.View
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsView.xaml
    /// </summary>
    public partial class ApplicationSettingsView : UserControl
    {
        public ApplicationSettingsView()
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
