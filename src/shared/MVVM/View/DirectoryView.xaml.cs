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
    public interface ISearchableView
    {
        void Activate();
    }

    /// <summary>
    ///     Interaction logic for DirectoryView.xaml
    /// </summary>
    public partial class DirectoryView : ISearchableView
    {
        private readonly DeferredAction m_deferredAction;
        private readonly TimeSpan m_delay;

        private ICollectionView m_view;

        public DirectoryView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            
            m_delay = TimeSpan.FromMilliseconds(100);
            m_deferredAction = DeferredAction.Create(ApplySearchCriteria);
        }

        public void Activate()
        {
            SearchTextBox.Focus();
        }

        private void OnLoaded(object _sender, RoutedEventArgs _e)
        {
            Activate();
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
            DirectoryViewModel viewModel = _sender as DirectoryViewModel;
            Debug.Assert(viewModel != null);

            UpdateFiles(viewModel);
        }

        private void UpdateFiles(DirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            ObservableRangeCollection<FileInfo> files = _directory.FileNames;

            ListCollectionView view = CollectionViewSource.GetDefaultView(files) as ListCollectionView;
            Debug.Assert(view != null);

            m_view = view;

            UpdateStatusBar(_directory);
        }

        private void UpdateStatusBar(DirectoryViewModel _directory)
        {
            Debug.Assert(_directory != null);

            int files = _directory.FileNames.Count;

            int selected = FilesDataGrid.SelectedItemsList?.Count ?? 0;

            string dir = _directory.Name;
            int count = m_view?.Cast<object>().Count() ?? 0;
            StatusTextBlock.Text = GetUpdateString(dir, count, files, selected);
        }

        private void DataGrid_OnDoubleClickRow(object _sender, MouseButtonEventArgs _e)
        {
            DataGridRow row = _sender as DataGridRow;
            Debug.Assert(row != null);

            FileInfo fileInfo = row.DataContext as FileInfo;
            Debug.Assert(fileInfo != null);

            string fullName = fileInfo.FullName;
            Debug.Assert(!string.IsNullOrWhiteSpace(fullName));

            string argument = "/select, \"" + fullName + "\"";

            Process.Start("explorer.exe", argument);
        }

        private void SearchTextbox_OnTextChanged(object _sender, TextChangedEventArgs _e)
        {
            m_deferredAction.Defer(m_delay);
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

        private void ApplySearchCriteria()
        {
            //early out if datacontext is null
            if (m_view == null)
            {
                return;
            }

            ListCollectionView view = m_view as ListCollectionView;
            Debug.Assert(view != null);

            string text = SearchTextBox.Text.ToLowerInvariant();

            view.Filter = _obj => _obj is FileInfo entry && entry.Name.ToLowerInvariant().Contains(text);

            DirectoryViewModel directory = DataContext as DirectoryViewModel;
            Debug.Assert(directory != null);

            UpdateStatusBar(directory);
        }

        //messaging helpers

        private static string GetUpdateString(string _root, int _currentCount, int _totalCount, int _selectedCount)
        {
            string resultsMessage = $"{_currentCount} of {_totalCount} files";
            string selectedMessage = $"{_selectedCount} selected";

            string message = $"{_root}\t{resultsMessage}: {selectedMessage}";
            return message;
        }

        private void FilesDataGrid_OnSelectedCellsChanged(object _sender, SelectedCellsChangedEventArgs _e)
        {
            DirectoryViewModel directory = DataContext as DirectoryViewModel;
            Debug.Assert(directory != null);

            UpdateStatusBar(directory);
        }
    }
}
