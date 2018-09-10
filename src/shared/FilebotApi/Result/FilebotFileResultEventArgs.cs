using System;
using System.Diagnostics;

namespace FilebotApi.Result
{
    public class FilebotFileResultEventArgs : EventArgs
    {
        public FilebotFileResultEventArgs(FilebotFileResult _result)
        {
            Debug.Assert(_result != null);

            Result = _result;
        }

        public FilebotFileResult Result { get; }
    }
}