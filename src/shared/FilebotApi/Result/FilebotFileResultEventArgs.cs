using System;
using System.Diagnostics;

namespace FilebotApi.Result
{
    public class RenameResultEventArgs : EventArgs
    {
        public RenameResultEventArgs(RenameResult _result)
        {
            Debug.Assert(_result != null);

            Result = _result;
        }

        public RenameResult Result { get; }
    }
}