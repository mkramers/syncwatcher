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

        public MediaDirectoryViewModel(string _directory, MediaDirectoryType _type) : base(_directory)
        {
            Type = _type;
        }

        public MediaDirectoryType Type { get; }
    }
}
