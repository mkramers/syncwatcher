using System.IO;

namespace Common.IO
{
    public class IoUtilities
    {
        public static void DirectoryCopy(string _sourceDirName, string _destDirName, bool _copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(_sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + _sourceDirName);

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(_destDirName))
                Directory.CreateDirectory(_destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(_destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (_copySubDirs)
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(_destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, _copySubDirs);
                }
        }
    }
}