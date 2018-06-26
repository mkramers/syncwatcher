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
        public FilebotRecords()
        {
            Renamed = new ObservableRangeCollection<RenameResult>();
            Skipped = new ObservableRangeCollection<SkipResult>();
        }

        public static bool TryLoad(string _fileName, out FilebotRecords _records)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_fileName));

            _records = null;

            try
            {
                _records = Utilities.XmlDeserializeObject<FilebotRecords>(_fileName);
            }
            catch (Exception)
            {
                var message = $"Filebot records failed to load from {Path.GetFullPath(_fileName)}";
                Console.WriteLine(message);
            }

            return _records != null;
        }

        public static void Save(FilebotRecords _records, string _path)
        {
            Debug.Assert(_records != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_path));

            try
            {
                Utilities.XmlSerializeObject(_records, _path);
            }
            catch (Exception e)
            {
                var message = $"Filebot records failed to save to {Path.GetFullPath(_path)}";
                throw new XmlException(message, e);
            }
        }

        internal void Update(IEnumerable<RenameResult> _renameResults, IEnumerable<SkipResult> _skipResults)
        {
            Update(_renameResults);
            Update(_skipResults);

            Updated?.Invoke(this, EventArgs.Empty);
        }

        private void Update(IEnumerable<RenameResult> _results)
        {
            Debug.Assert(_results != null);

            var current = Renamed;
            foreach (var result in _results)
            {
                var existing = current.Where(_complete => _complete.OriginalFile == result.OriginalFile).ToList();
                if (existing.Any())
                    foreach (var existingResult in existing)
                        existingResult.DateTime = result.DateTime;
                else
                    current.Add(result);
            }
        }

        private void Update(IEnumerable<SkipResult> _results)
        {
            Debug.Assert(_results != null);

            var current = Skipped;
            foreach (var result in _results)
            {
                var existing = current.Where(_complete => _complete.OriginalFile == result.OriginalFile).ToList();
                if (existing.Any())
                    foreach (var existingResult in existing)
                        existingResult.DateTime = result.DateTime;
                else
                    current.Add(result);
            }
        }

        public ICommand RequestRefreshCommand => new RelayCommand(() => RequestRefresh?.Invoke(this, EventArgs.Empty));

        public event EventHandler<EventArgs> Updated;
        public event EventHandler<EventArgs> RequestRefresh;

        public ObservableRangeCollection<RenameResult> Renamed { get; }
        public ObservableRangeCollection<SkipResult> Skipped { get; }
    }
}