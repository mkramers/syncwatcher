using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FilebotApi.Result;

namespace FilebotApi.View
{
    /// <summary>
    ///     Interaction logic for RenameResultView.xaml
    /// </summary>
    public partial class RenameResultView : UserControl
    {
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
                var renamed = records.Renamed;

                m_renamedView = CollectionViewSource.GetDefaultView(renamed);
                m_renamedView.Filter = Filter;
            }
        }

        private void SearchTextbox_OnTextChanged(object _sender, TextChangedEventArgs _e)
        {
            var current = ResultsDataGrid.SelectedItem;

            m_renamedView.Refresh();

            var results = m_renamedView.Cast<RenameResult>().ToArray();

            ResultsDataGrid.SelectedItem = results.Contains(current) ? current : results.FirstOrDefault();
        }

        private void SearchTextBox_OnKeyDown(object _sender, KeyEventArgs _e)
        {
            var key = _e.Key;
            switch (key)
            {
                case Key.Escape:
                    SearchTextBox.Text = "";
                    break;
            }
        }

        private void OnRecordsUpdated(object _sender, EventArgs _e)
        {
            var view = m_renamedView;
            Debug.Assert(view != null);

            var dispatcher = Dispatcher;
            dispatcher.Invoke(() =>
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
            var accepted = false;

            if (_item is RenameResult result)
            {
                var search = SearchTextBox.Text.ToLowerInvariant();

                var name = result.ProposedFileName.ToLowerInvariant();

                accepted = name.Contains(search);
            }

            return accepted;
        }

        private ICollectionView m_renamedView;
    }
}