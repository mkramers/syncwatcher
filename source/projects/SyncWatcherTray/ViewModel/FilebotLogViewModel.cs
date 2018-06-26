using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Common.API;
using GalaSoft.MvvmLight;

namespace SyncWatcherTray.ViewModel
{
    public class FilebotLogViewModel : ViewModelBase
    {
        public FilebotLogViewModel(FilebotLog _log)
        {
            Debug.Assert(_log != null);

            Log = _log;
            Log.Updated += LogUpdated;
        }

        private void LogUpdated(object _sender, EventArgs _e)
        {
            var recordManager = _sender as FilebotLog;
            Debug.Assert(recordManager != null);

            RaisePropertyChanged("Entries");
        }

        public ObservableCollection<string> Entries => new ObservableCollection<string>(Log.Entries);
        
        public FilebotLog Log { get; set; }
    }
}
