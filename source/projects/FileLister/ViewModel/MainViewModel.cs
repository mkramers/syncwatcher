using System.Diagnostics;
using System.IO;
using FilebotApi;
using FileLister.Model;
using GalaSoft.MvvmLight;

namespace FileLister.ViewModel
{
    /// <summary>
    ///     This class contains properties that the main View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm
    ///     </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
                return;

            var path = Paths.SettingsFile;
            if (!MultiFileLister.FileListerSettings.TryLoad(path, out var settings))
                Debug.Fail($"error loading app settings! {path}");

            var settingsPath = Paths.FileBotSettings;
            var recordsPath = Paths.FilebotRecords;

            if (!File.Exists(settingsPath))
                FilebotSettings.CreateDefaultSettingsFile(settingsPath);

            if (!Filebot.TryCreate(settingsPath, recordsPath, out var filebot))
                Debug.Fail($"error loading filebot {Path.GetFullPath(settingsPath)}");

            DirectoryModelCache.Instance.Initialize(Paths.AppData);

            var model = new MultiFileLister(filebot, settings, Paths.AppData);

            ViewModel = new MultiFileViewerViewModel(model);
        }

        public ViewModelBase ViewModel
        {
            get => m_viewModel;
            set
            {
                m_viewModel = value;
                RaisePropertyChanged();
            }
        }

        private ViewModelBase m_viewModel;
    }
}