using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using FilebotApi.Result;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;

namespace FilebotApi
{
    public class FilebotHistory
    {
        private ICollectionView m_entriesView;
        private string m_historyFilePath;

        public ObservableRangeCollection<RenameResult> Entries { get; }
        public RelayCommand ClearHistoryCommand => new RelayCommand(ClearHistory);

        public ICollectionView EntriesView => m_entriesView ?? (m_entriesView = CollectionViewSource.GetDefaultView(Entries));

        public FilebotHistory()
        {
            Entries = new ObservableRangeCollection<RenameResult>();
            EntriesView.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
        }

        public void Load(string _historyFilePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_historyFilePath));

            m_historyFilePath = _historyFilePath;

            Reload();
        }

        public void Reload()
        {
            string serializedObject = File.ReadAllText(m_historyFilePath);

            IEnumerable<RenameResult> entries = JsonConvert.DeserializeObject<IEnumerable<RenameResult>>(serializedObject);

            Entries.Clear();
            Entries.AddRange(entries);
        }

        public void Save()
        {
            string serializedObject = JsonConvert.SerializeObject(Entries, Formatting.Indented);

            File.WriteAllText(m_historyFilePath, serializedObject);
        }

        private void ClearHistory()
        {
            Entries.Clear();
            Save();
        }

        public void AddEntry(RenameResult _result)
        {
            Debug.Assert(_result != null);

            //todo do existing find/replace
            RenameResult existing = Entries.Find(_entry => string.Equals(_entry.ProposedFileName, _result.ProposedFileName, StringComparison.OrdinalIgnoreCase));

            //remove existing and add updated
            if (existing != null)
            {
                Entries.Remove(existing);
            }

            Entries.Add(_result);

            Save();
        }
    }
}
