using System.Reflection;
using System.Windows;
using log4net;

namespace FileLister
{
    public partial class App
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
        }

        private static readonly ILog LOG = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}