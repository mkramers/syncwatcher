using System.Collections.Generic;
using System.Diagnostics;

namespace FileLister.Model
{
    public partial class MultiFileLister
    {
        public class FileListerSettings
        {
            public FileListerSettings()
            {
                RootLists = new List<List<string>>();
            }

            public FileListerSettings(List<List<string>> _rootLists)
            {
                Debug.Assert(_rootLists != null);

                RootLists = new List<List<string>>(_rootLists);
            }

            public static bool TryCreateDefault(string _filePath)
            {
                Debug.Assert(_filePath != null);

                return Default.TrySave(_filePath);
            }

            public bool TrySave(string _filePath)
            {
                Debug.Assert(_filePath != null);

                var success = false;
                try
                {
                    Common.Utilities.XmlSerializeObject(this, _filePath);
                    success = true;
                }
                catch
                {
                    success = false;
                }
                return success;
            }

            public static bool TryLoad(string _filePath, out FileListerSettings _settings)
            {
                Debug.Assert(_filePath != null);

                _settings = null;

                var success = false;
                try
                {
                    _settings = Common.Utilities.XmlDeserializeObject<FileListerSettings>(_filePath);
                    success = _settings != null;
                }
                catch
                {
                }

                return success;
            }

            public static FileListerSettings Default
            {
                get
                {
                    var rootLists = new List<List<string>>
                    {
                        new List<string>
                        {
                            @"D:/Unsorted/completed"
                        },
                        new List<string>
                        {
                            @"F:/videos"
                        }
                    };

                    var settings = new FileListerSettings(rootLists);
                    return settings;
                }
            }

            public List<List<string>> RootLists { get; set; }
        }
    }
}