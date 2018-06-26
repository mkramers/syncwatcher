using System.Collections.Generic;

namespace Common.SFTP
{
    public class FtpSessionConfig
    {
        public string HistoryFilePath { get; set; }
        public int WatchPeriod { get; set; }
        public int NumberOfSlots { get; set; }
        public string SshHostKeyFingerprint { get; set; }
        public string Host { get; set; }
        public bool DeleteSource { get; set; }
        public List<string> RemoteRoots { get; set; }

        public static FtpSessionConfig Default => new FtpSessionConfig
        {
            HistoryFilePath = "..\\..\\history.xml",
            WatchPeriod = 10000,
            NumberOfSlots = 4,
            SshHostKeyFingerprint = "ssh-rsa 2048 13:bd:c2:98:f6:9d:53:f7:1a:6a:38:9d:14:2e:1f:7a",
            Host = "mole.seedhost.eu",
            DeleteSource = false,
            RemoteRoots = new List<string>
            {
                "/home6/emptycup/downloads/completed/test",
                "/home6/emptycup/downloads/test2",
                "/home6/emptycup/downloads/completed"
            }
        };
    }
}