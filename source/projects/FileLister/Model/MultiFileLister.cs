using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FilebotApi;

namespace FileLister.Model
{
    public partial class MultiFileLister
    {
        public MultiFileLister(Filebot _fileBot, FileListerSettings _settings, string _appDataDirectory)
        {
            Debug.Assert(_fileBot != null);
            Debug.Assert(_settings != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_appDataDirectory));
            Debug.Assert(Directory.Exists(_appDataDirectory));

            FileBot = _fileBot;
            FileBot.Stopped += FileBot_Event;

            Settings = _settings;

            m_appDataDirectory = _appDataDirectory;

            //populate filelist models
            FileListers = new List<DirectoriesModel>();
            foreach (var rootList in Settings.RootLists)
                FileListers.Add(new DirectoriesModel(rootList));
        }

        private void FileBot_Event(object _sender, EventArgs _e)
        {
            FileBotLogUpdated?.Invoke(_sender, _e);
        }

        internal async void Organize(string _inputDir, string _outputDir)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_inputDir));
            Debug.Assert(!string.IsNullOrWhiteSpace(_outputDir));

            await Task.Factory.StartNew(() => FileBot.Organize(_inputDir, _outputDir));

            FileBotCompleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> FileBotLogUpdated;
        public event EventHandler<EventArgs> FileBotCompleted;

        public Filebot FileBot { get; }
        public FileListerSettings Settings { get; }
        public List<DirectoriesModel> FileListers { get; }

        private readonly string m_appDataDirectory;
    }
}