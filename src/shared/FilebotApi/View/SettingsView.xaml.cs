using System.Diagnostics;
using System.Windows.Controls;
using FilebotApi.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace FilebotApi.View
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
            Settings filebot = DataContext as Settings;
            Debug.Assert(filebot != null);

            filebot.Save();
        }
    }
}
