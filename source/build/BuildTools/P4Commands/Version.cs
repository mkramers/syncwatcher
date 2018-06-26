using System.Diagnostics;

namespace P4Commands
{
    public class Version
    {
        public Version()
        {
        }

        public Version(int _major, int _minor, int? _revision) : this(_major, _minor, _revision, null)
        {
        }

        public Version(int _major, int _minor, int? _revision, int? _changelist)
        {
            Major = _major;
            Minor = _minor;
            Revision = _revision;
            Changelist = _changelist;
        }

        public Version(int _major, int _minor) : this(_major, _minor, null)
        {
        }

        public static bool TryParse(string _versionRaw, out Version _version)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_versionRaw));

            _version = null;

            var split = _versionRaw.Split('.');
            if (split.Length >= 2)
            {
                var majorRaw = split[0];
                var minorRaw = split[1];

                string revisionRaw = null;
                if (split.Length >= 3)
                {
                    revisionRaw = split[2];
                }

                string changelistRaw = null;
                if (split.Length >= 4)
                {
                    changelistRaw = split[3];
                }

                int major;
                if (int.TryParse(majorRaw, out major))
                {
                    int minor;
                    if (int.TryParse(minorRaw, out minor))
                    {
                        _version = new Version(major, minor);

                        int revision;
                        if (revisionRaw != null && int.TryParse(revisionRaw, out revision))
                        {
                            _version.Revision = revision;

                            int changelist;
                            if (changelistRaw != null && int.TryParse(changelistRaw, out changelist))
                            {
                                _version.Changelist = changelist;
                            }
                        }
                    }
                }
            }

            return _version != null;
        }

        public override string ToString()
        {
            var revision = Revision?.ToString() ?? "X";
            var changelist = Changelist?.ToString() ?? "X";
            return $"{Major}.{Minor}.{revision}.{changelist}";
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public int? Revision { get; set; }
        public int? Changelist { get; set; }
    }
}