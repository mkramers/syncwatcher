using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FilebotApi.Result;
using Newtonsoft.Json;

namespace FilebotApi
{
    public class FilebotHistory
    {
        public FilebotHistory()
        {
            Entries = new ObservableRangeCollection<RenameResult>();
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

        public ObservableRangeCollection<RenameResult> Entries { get; }
        private string m_historyFilePath;

        public void AddEntry(RenameResult _result)
        {
            Debug.Assert(_result != null);

            //todo do existing find/replace
            Entries.Add(_result);

            Save();
        }
    }
}