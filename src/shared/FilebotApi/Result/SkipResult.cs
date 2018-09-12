using System;

namespace FilebotApi.Result
{
    public class SkipResult : FilebotFileResult
    {
        public SkipResult()
        {
        }

        public SkipResult(string _originalFile, string _rawLine, DateTime _dateTime) : base(_originalFile, _rawLine, _dateTime)
        {
        }
    }
}
