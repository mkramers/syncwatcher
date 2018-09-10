using System;
using System.Diagnostics;
using System.IO;

namespace FilebotApi.Result
{
    public class FilebotFileResult : FileBotResult
    {
        public string OriginalFile { get; set; }
        public string OriginalFileName => Path.GetFileName(OriginalFile);

        public FilebotFileResult()
        {
        }

        public FilebotFileResult(string _originalFile, string _rawLine, DateTime _dateTime) : base(_rawLine, _dateTime)
        {
            Debug.Assert(_originalFile != null);

            OriginalFile = _originalFile;
        }
    }
}
