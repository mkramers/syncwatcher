using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Common.Framework;
using MVVM.ViewModel;

namespace MVVM.View
{
    /// <summary>
    ///     Interaction logic for DirectoryView.xaml
    /// </summary>
    public partial class DirectoryView
    {
        public DirectoryView()
        {
            InitializeComponent();

            m_delay = TimeSpan.FromMilliseconds(100);
            m_deferredAction = DeferredAction.Create(ApplySearchCriteria);
        }

        private void OnDataContextChanged(object _sender, DependencyPropertyChangedEventArgs _e)
        {
            if (_e.OldValue is DirectoryViewModel oldDirectory)
            {
                oldDirectory.Updated -= Directory_OnUpdated;
                m_view = null;
            }

            if (_e.NewValue is DirectoryViewModel newDirectory)
            {
                newDirectory.Updated += Directory_OnUpdated;
                UpdateFiles(newDirectory);
            }
        }

        private void Directory_OnUpdated(object _sender, EventArgs _eventArgs)
        {
            var viewModel = _sender as DirectoryViewModel;
            Debug.Assert(viewModel != null);

            UpdateFiles(viewModel);
        }

        private void UpdateFiles(DirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            var files = _directory.FileNames;

            var view = CollectionViewSource.GetDefaultView(files) as ListCollectionView;
            Debug.Assert(view != null);

            m_view = view;

            UpdateStatusBar(_directory);
        }

        private void UpdateStatusBar(DirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            var files = _directory.FileNames.Count;

            var selected = FilesDataGrid.SelectedItemsList?.Count ?? 0;

            var dir = _directory.Name;
            var count = m_view?.Cast<object>().Count() ?? 0;
            StatusTextBlock.Text = GetUpdateString(dir, count, files, selected);
        }

        private void DataGrid_OnDoubleClickRow(object _sender, MouseButtonEventArgs _e)
        {
            var row = _sender as DataGridRow;
            Debug.Assert(row != null);

            var fileInfo = row.DataContext as FileInfo;
            Debug.Assert(fileInfo != null);

            var fullName = fileInfo.FullName;
            Debug.Assert(!string.IsNullOrWhiteSpace(fullName));

            var argument = "/select, \"" + fullName + "\"";

            Process.Start("explorer.exe", argument);
        }

        private void SearchTextbox_OnTextChanged(object _sender, TextChangedEventArgs _e)
        {
            m_deferredAction.Defer(m_delay);
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

        private void ApplySearchCriteria()
        {
            //early out if datacontext is null
            if (m_view == null)
            {
                return;
            }

            var view = m_view as ListCollectionView;
            Debug.Assert(view != null);

            var text = SearchTextBox.Text.ToLowerInvariant();

            view.Filter = _obj => _obj is FileInfo entry && entry.Name.ToLowerInvariant().Contains(text);

            var directory = DataContext as DirectoryViewModel;
            Debug.Assert(directory != null);

            UpdateStatusBar(directory);
        }

        //messaging helpers

        private static string GetUpdateString(string _root, int _currentCount, int _totalCount, int _selectedCount)
        {
            var resultsMessage = $"{_currentCount} of {_totalCount} files";
            var selectedMessage = $"{_selectedCount} selected";

            var message = $"{_root}\t{resultsMessage}: {selectedMessage}";
            return message;
        }

        private ICollectionView m_view;
        private readonly DeferredAction m_deferredAction;
        private readonly TimeSpan m_delay;

        private void FilesDataGrid_OnSelectedCellsChanged(object _sender, SelectedCellsChangedEventArgs _e)
        {
            var directory = DataContext as DirectoryViewModel;
            Debug.Assert(directory != null);

            UpdateStatusBar(directory);
        }
    }
}