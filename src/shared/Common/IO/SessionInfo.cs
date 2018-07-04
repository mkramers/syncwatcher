//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WinSCP;

//namespace Common
//{

//    public class SessionInfo
//    {
//        public void Load()
//        {
//            try
//            {
//                // Setup session options
//                SessionOptions = new SessionOptions
//                {
//                    Protocol = Protocol.Ftp,
//                    HostName = "ns513998.dediseedbox.com",
//                    UserName = "krames",
//                    Password = "8d0e2dbcfe",
//                };
//            }
//            catch (Exception e)
//            {
//                LOG.ErrorFormat("Error: {0}", e);
//            }
//        }

//        public SessionOptions SessionOptions { get; private set; }

//        //the once-per-class call to initialize the log4net object
//        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//    }
//}

