using System;
using System.Diagnostics;

namespace FilebotApi.Result
{
    public class RenameResultEventArgs : EventArgs
    {
        public RenameResult Result { get; }

        public RenameResultEventArgs(RenameResult _result)
        {
            Debug.Assert(_result != null);

            Result = _result;
        }
    }
}
