using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;

namespace CakeExtensions
{
    public static class CakeExtensions
    {
        [CakeMethodAlias]
        public static void GetVersionNumber(this ICakeContext _context, string _gitVersion, string _branchName, string _buildNumber, out string _shortVersion, out string _longVersion)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_gitVersion));
            Debug.Assert(!string.IsNullOrWhiteSpace(_branchName));
            Debug.Assert(!string.IsNullOrWhiteSpace(_buildNumber));

            GetVersionStrings(_gitVersion, _branchName, _buildNumber, out _shortVersion, out _longVersion);
        }

        internal static void GetVersionStrings(string _gitVersion, string _branchName, string _buildNumber, out string _shortVersion, out string _longVersion)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_gitVersion));
            Debug.Assert(!string.IsNullOrWhiteSpace(_branchName));
            Debug.Assert(!string.IsNullOrWhiteSpace(_buildNumber));

            string[] gitVersionSplit = _gitVersion.Split('-');

            string gitLabel;
            string majorMinor;

            if (gitVersionSplit.Length == 1)
            {
                majorMinor = "0.0.0";

                //occurs when there are no tags present
                Console.WriteLine($"No git tag present. Using v {majorMinor}");

                gitLabel = _gitVersion;
            }
            else if (gitVersionSplit.Length >= 3)
            {
                string tag = gitVersionSplit[0]; //i.e. bx_1.1 (or the old v1.0.0 style)

                string[] splitTag = tag.Split('_');
                if (splitTag.Length == 1)
                {
                    //no splitting, assume old tag format of v1.0.0
                    majorMinor = tag.Substring(1, tag.Length - 1);
                }
                else if (splitTag.Length > 1)
                {
                    //is split by _, assume new tag format of name_of_release_1.0.0
                    majorMinor = splitTag.Last();
                }
                else
                {
                    throw new Exception($"Failed to parse git tag from {_gitVersion}");
                }

                //determine if the label contains 2 or 3 numbers (i.e. 1.1 vs 1.1.1)
                //this is to support old tags. moving forward we should assert/throw here
                if (majorMinor.Split('.').Length == 2)
                {
                    majorMinor = $"{majorMinor}.0";
                }

                gitLabel = string.Join("-", gitVersionSplit.Skip(1)); //i.e. 80-gdb791cd
            }
            else
            {
                throw new Exception($"Failed to parse git label from {_gitVersion}");
            }

            _shortVersion = $"{majorMinor}.{_buildNumber}"; //i.e. 1.1.0.666
            _longVersion = $"{_shortVersion}.{gitLabel}!-{_branchName}";
        }

        [CakeMethodAlias]
        public static string ReplaceIllegalChars(this ICakeContext _context, string _input, string _replacement)
        {
            Debug.Assert(!string.IsNullOrEmpty(_input));
            Debug.Assert(_replacement != null);

            char[] invalidChars = Path.GetInvalidFileNameChars();

            string validEntry = invalidChars.Aggregate(_input, (_current, _c) => _current.Replace(_c.ToString(), _replacement));
            return validEntry;
        }
    }
}