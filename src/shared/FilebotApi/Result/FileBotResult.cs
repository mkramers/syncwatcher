using System;
using System.Diagnostics;

namespace FilebotApi.Result
{
    public class FileBotResult
    {
        public string RawLine { get; set; }
        public DateTime DateTime { get; set; }

        public FileBotResult()
        {
        }

        public FileBotResult(string _rawLine, DateTime _dateTime)
        {
            Debug.Assert(_rawLine != null);

            RawLine = _rawLine;
            DateTime = _dateTime;
        }
    }
}
