using System.Diagnostics;
using System.Windows.Controls;
using Common.SFTP;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace WinScpApi.View
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void PropertyGrid_OnPropertyValueChanged(object _sender, PropertyValueChangedEventArgs _e)
        {
            SessionConfig sessionConfig = DataContext as SessionConfig;
            Debug.Assert(sessionConfig != null);

            sessionConfig.Save();
        }
    }
}
