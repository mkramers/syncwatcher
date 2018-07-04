using System.IO;

namespace FileLister.ViewModel
{
    public class FileRowViewModel
    {
        public FileInfo FileInfo { get; set; }
        public bool IsSelected { get; set; }
    }
}