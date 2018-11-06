using MVVM.View;

namespace SyncWatcherTray.View
{
    /// <summary>
    ///     Interaction logic for DirectoryView.xaml
    /// </summary>
    public partial class LocalCleanerView : ISearchableView
    {
        public LocalCleanerView()
        {
            InitializeComponent();
        }

        public void Activate()
        {
            DirectoryView.Activate();
        }
    }
}
