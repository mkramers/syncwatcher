using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FilebotApi.Result;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;

namespace FilebotApi
{
    public class FilebotHistory
    {
        public FilebotHistory()
        {
            Entries = new ObservableRangeCollection<FilebotFileResult>();
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

            IEnumerable<FilebotFileResult> entries = JsonConvert.DeserializeObject<IEnumerable<FilebotFileResult>>(serializedObject);

            Entries.Clear();
            Entries.AddRange(entries);
        }

        public void Save()
        {
            string serializedObject = JsonConvert.SerializeObject(Entries, Formatting.Indented);

            File.WriteAllText(m_historyFilePath, serializedObject);
        }

        public ObservableRangeCollection<FilebotFileResult> Entries { get; }
        private string m_historyFilePath;
    }

    public class FilebotLog
    {
        public ICommand ReloadCommand => new RelayCommand(Reload);

        public ObservableRangeCollection<RenameResult> Renamed { get; }
        public ObservableRangeCollection<SkipResult> Skipped { get; }
        private string RecordsFilePath { get; }

        public FilebotLog(string _recordsFilePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_recordsFilePath));

            RecordsFilePath = _recordsFilePath;

            Renamed = new ObservableRangeCollection<RenameResult>();
            Skipped = new ObservableRangeCollection<SkipResult>();

            Reload();
        }

        public event EventHandler<EventArgs> Updated;

        public void Reload()
        {
            string recordsPath = RecordsFilePath;

            Debug.Assert(!string.IsNullOrWhiteSpace(recordsPath));

            List<RenameResult> renameResults = new List<RenameResult>();
            List<SkipResult> skipResults = new List<SkipResult>();

            if (File.Exists(recordsPath))
            {
                string[] lines = File.ReadAllLines(recordsPath);
                foreach (string line in lines)
                {
                    if (FileBotLogParser.TryParse(line, out FileBotResult result))
                    {
                        switch (result)
                        {
                            case RenameResult renameResult:
                                renameResults.Add(renameResult);
                                break;
                            case SkipResult skipResult:
                                skipResults.Add(skipResult);
                                break;
                        }
                    }
                }
            }

            Update(renameResults, skipResults);
        }

        private void Update(IEnumerable<RenameResult> _renameResults, IEnumerable<SkipResult> _skipResults)
        {
            Update(_renameResults);
            Update(_skipResults);

            Updated?.Invoke(this, EventArgs.Empty);
        }

        private void Update(IEnumerable<RenameResult> _results)
        {
            Debug.Assert(_results != null);

            ObservableRangeCollection<RenameResult> current = Renamed;
            foreach (RenameResult result in _results)
            {
                List<RenameResult> existing = current.Where(_complete => _complete.OriginalFile == result.OriginalFile).ToList();
                if (existing.Any())
                {
                    foreach (RenameResult existingResult in existing)
                    {
                        existingResult.DateTime = result.DateTime;
                    }
                }
                else
                {
                    current.Add(result);
                }
            }
        }

        private void Update(IEnumerable<SkipResult> _results)
        {
            Debug.Assert(_results != null);

            ObservableRangeCollection<SkipResult> current = Skipped;
            foreach (SkipResult result in _results)
            {
                List<SkipResult> existing = current.Where(_complete => _complete.OriginalFile == result.OriginalFile).ToList();
                if (existing.Any())
                {
                    foreach (SkipResult existingResult in existing)
                    {
                        existingResult.DateTime = result.DateTime;
                    }
                }
                else
                {
                    current.Add(result);
                }
            }
        }
    }
}
