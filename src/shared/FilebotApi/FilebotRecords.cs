using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using Common;
using FilebotApi.Result;
using GalaSoft.MvvmLight.CommandWpf;

namespace FilebotApi
{
    public class FilebotRecords
    {
        public ICommand RequestRefreshCommand => new RelayCommand(() => RequestRefresh?.Invoke(this, EventArgs.Empty));

        public ObservableRangeCollection<RenameResult> Renamed { get; }
        public ObservableRangeCollection<SkipResult> Skipped { get; }
        public string RecordsFilePath { get; }

        public event EventHandler<EventArgs> Updated;
        public event EventHandler<EventArgs> RequestRefresh;

        public FilebotRecords(string _recordsFilePath)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_recordsFilePath));

            RecordsFilePath = _recordsFilePath;

            Renamed = new ObservableRangeCollection<RenameResult>();
            Skipped = new ObservableRangeCollection<SkipResult>();
        }

        public static bool TryLoad(string _fileName, out FilebotRecords _records)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(_fileName));

            _records = null;

            try
            {
                _records = Utilities.XmlDeserializeObject<FilebotRecords>(_fileName);
            }
            catch (Exception)
            {
                string message = $"Filebot records failed to load from {Path.GetFullPath(_fileName)}";
                Console.WriteLine(message);
            }

            return _records != null;
        }

        public static void Save(FilebotRecords _records, string _path)
        {
            Debug.Assert(_records != null);
            Debug.Assert(!String.IsNullOrWhiteSpace(_path));

            try
            {
                Utilities.XmlSerializeObject(_records, _path);
            }
            catch (Exception e)
            {
                string message = $"Filebot records failed to save to {Path.GetFullPath(_path)}";
                throw new XmlException(message, e);
            }
        }

        public void Reload()
        {
            string recordsPath = RecordsFilePath;

            Debug.Assert(!String.IsNullOrWhiteSpace(recordsPath));

            if (!File.Exists(recordsPath))
                return;

            string[] lines = File.ReadAllLines(recordsPath);
            List<RenameResult> renameResults = new List<RenameResult>();
            List<SkipResult> skipResults = new List<SkipResult>();
            foreach (string line in lines)
                if (FileBotLogParser.TryParse(line, out FileBotResult result))
                    switch (result)
                    {
                        case RenameResult renameResult:
                            renameResults.Add(renameResult);
                            break;
                        case SkipResult skipResult:
                            skipResults.Add(skipResult);
                            break;
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
                    foreach (RenameResult existingResult in existing)
                        existingResult.DateTime = result.DateTime;
                else
                    current.Add(result);
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
                    foreach (SkipResult existingResult in existing)
                        existingResult.DateTime = result.DateTime;
                else
                    current.Add(result);
            }
        }
    }
}