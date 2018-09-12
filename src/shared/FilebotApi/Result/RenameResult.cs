using System;
using System.Diagnostics;
using System.IO;

namespace FilebotApi.Result
{
    public class RenameResult : FilebotFileResult

    {
        public RenameResult()
        {
        }

        public RenameResult(Filebot.ActionType _mode, string _originalFile, string _proposedFile, string _rawLine,
            DateTime _dateTime) : base(_originalFile, _rawLine, _dateTime)
        {
            Debug.Assert(_proposedFile != null);

            Mode = _mode;
            ProposedFile = _proposedFile;
        }

        public Filebot.ActionType Mode { get; set; }
        public string ProposedFile { get; set; }
        public string ProposedFileName => Path.GetFileName(ProposedFile);
    }
}