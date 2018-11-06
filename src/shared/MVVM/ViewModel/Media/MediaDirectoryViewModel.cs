namespace MVVM.ViewModel.Media
{
    public enum MediaDirectoryType
    {
        MOVIES,
        SERIES
    }

    public class MediaDirectoryViewModel : DirectoryViewModel
    {
        public MediaDirectoryViewModel()
        {
        }

        public MediaDirectoryViewModel(string _directory, string _shortName, MediaDirectoryType _type) : base(_directory, _shortName)
        {
            Type = _type;
        }

        public MediaDirectoryType Type { get; }
    }
}
