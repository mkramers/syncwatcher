using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using FilebotApi.Result;

namespace FilebotApi.View
{
    /// <summary>
    ///     Interaction logic for RenameResultView.xaml
    /// </summary>
    public partial class RenameResultView : UserControl
    {
        private ICollectionView m_renamedView;

        public RenameResultView()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object _sender, DependencyPropertyChangedEventArgs _e)
        {
            if (_e.OldValue is FilebotLog oldRecords)
            {
                oldRecords.Updated -= OnRecordsUpdated;
            }

            if (_e.NewValue is FilebotLog records)
            {
                records.Updated += OnRecordsUpdated;

                // Collection which will take your Filter
                ObservableRangeCollection<RenameResult> renamed = records.Renamed;

                m_renamedView = CollectionViewSource.GetDefaultView(renamed);
                m_renamedView.Filter = Filter;
            }
        }

        private void SearchTextbox_OnTextChanged(object _sender, TextChangedEventArgs _e)
        {
            object current = ResultsDataGrid.SelectedItem;

            m_renamedView.Refresh();

            RenameResult[] results = m_renamedView.Cast<RenameResult>().ToArray();

            ResultsDataGrid.SelectedItem = results.Contains(current) ? current : results.FirstOrDefault();
        }

        private void SearchTextBox_OnKeyDown(object _sender, KeyEventArgs _e)
        {
            Key key = _e.Key;
            switch (key)
            {
                case Key.Escape:
                    SearchTextBox.Text = "";
                    break;
            }
        }

        private void OnRecordsUpdated(object _sender, EventArgs _e)
        {
            ICollectionView view = m_renamedView;
            Debug.Assert(view != null);

            Dispatcher dispatcher = Dispatcher;
            dispatcher.Invoke(
                () =>
                {
                    if (view.CanSort)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
                    }
                });
        }

        private bool Filter(object _item)
        {
            bool accepted = false;

            if (_item is RenameResult result)
            {
                string search = SearchTextBox.Text.ToLowerInvariant();

                string name = result.ProposedFileName.ToLowerInvariant();

                accepted = name.Contains(search);
            }

            return accepted;
        }
    }
}
