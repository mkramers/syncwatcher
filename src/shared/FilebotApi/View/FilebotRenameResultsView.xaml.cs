using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Win32;
using FilebotApi.Result;

namespace FilebotApi.View
{
    /// <summary>
    ///     Interaction logic for FilebotRenameResultsView.xaml
    /// </summary>
    public partial class FilebotRenameResultsView
    {
        public FilebotRenameResultsView()
        {
            InitializeComponent();
        }

        private void DataGrid_OnDoubleClickRow(object _sender, MouseButtonEventArgs _e)
        {
            DataGridRow row = _sender as DataGridRow;
            Debug.Assert(row != null);

            RenameResult renameResult = row.DataContext as RenameResult;
            Debug.Assert(renameResult != null);

            string fullName = renameResult.ProposedFile;
            Debug.Assert(!string.IsNullOrWhiteSpace(fullName));

            OpenInExplorer.Open(fullName);
        }
    }
}
