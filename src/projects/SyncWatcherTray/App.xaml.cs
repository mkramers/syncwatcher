using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
using log4net.Config;
using MVVM.Popups;
using SyncWatcherTray.ViewModel;

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace SyncWatcherTray
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs _e)
        {
            base.OnStartup(_e);

            var logConfig = new FileInfo("Config/log4net.config");
            GlobalContext.Properties["LogFileName"] = "log.txt";
            XmlConfigurator.Configure(logConfig);

            Log.Info("Starting SyncWatcherTray");

            m_viewModel = new MainViewModel();

            m_mainWindow = new View.MainWindow();
            m_mainWindow.InitializeComponent();
            m_mainWindow.DataContext = m_viewModel;

            MainWindow = m_mainWindow;

#if !DEBUG
            m_mainWindow.ShowWindow();
#endif
        }

        protected override void OnExit(ExitEventArgs _e)
        {
            m_viewModel.Cleanup();

            PopupManager.Instance.Exit();

            base.OnExit(_e);
        }

        private MainViewModel m_viewModel;
        private View.MainWindow m_mainWindow;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}