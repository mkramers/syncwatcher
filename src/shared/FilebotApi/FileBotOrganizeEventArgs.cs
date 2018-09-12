using System;
using FilebotApi.Result;

namespace FilebotApi
{
    public class FileBotOrganizeEventArgs : EventArgs
    {
        public OrganizeResult Result { get; }

        public FileBotOrganizeEventArgs(OrganizeResult _result)
        {
            Result = _result;
        }
    }
}
