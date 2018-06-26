//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WinSCP;

//namespace Common
//{
//    public class RemoteFileInfo
//    {
//        public RemoteFileInfo()
//        {
//        }

//        public RemoteFileInfo(WinSCP.RemoteFileInfo _remoteFileInfo)
//        {
//            try
//            {
//                FilePermissions = _remoteFileInfo.FilePermissions;
//                FileType = _remoteFileInfo.FileType;
//                Group = _remoteFileInfo.Group;
//                IsDirectory = _remoteFileInfo.IsDirectory;
//                LastWriteTime = _remoteFileInfo.LastWriteTime;
//                Length = _remoteFileInfo.Length;
//                Length32 = _remoteFileInfo.Length32;
//                Name = _remoteFileInfo.Name;
//                Owner = _remoteFileInfo.Owner;
//            }
//            catch (Exception e)
//            {
//                if (e.GetType() != typeof(OverflowException))
//                {
//                    LOG.WarnFormat("Error: {0} : {1}", e.Message, e.StackTrace);
//                }
//            }
//        }

//        public override string ToString()
//        {
//            return Name;
//        }

//        public FilePermissions FilePermissions { get; set; }
//        public char FileType { get; set; }
//        public string Group { get; set; }
//        public bool IsDirectory { get; set; }
//        public DateTime LastWriteTime { get; set; }
//        public long Length { get; set; }
//        public int Length32 { get; set; }
//        public string Name { get; set; }
//        public string Owner { get; set; }
//        public string Extension
//        {
//            get
//            {
//                return Utilities.GetExtensionFromFileName(Name);
//            }
//        }

//        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//    }
//}

