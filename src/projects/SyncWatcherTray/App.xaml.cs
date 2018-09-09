using System;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;
using MVVM.Popups;
using SyncWatcherTray.View;
using SyncWatcherTray.ViewModel;

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace SyncWatcherTray
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MainWindow m_mainWindow;

        private MainViewModel m_viewModel;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void OnStartup(StartupEventArgs _e)
        {
            base.OnStartup(_e);

            FileInfo logConfig = new FileInfo("Config/log4net.config");
            GlobalContext.Properties["LogFileName"] = "log.txt";
            XmlConfigurator.Configure(logConfig);

            Log.Info("Starting SyncWatcherTray");

            m_viewModel = new MainViewModel();
            m_viewModel.Initialize();

            m_mainWindow = new MainWindow();
            m_mainWindow.InitializeComponent();
            m_mainWindow.DataContext = m_viewModel;

            MainWindow = m_mainWindow;

#if DEBUG
            m_mainWindow.ShowWindow();
#endif
        }

        protected override void OnExit(ExitEventArgs _e)
        {
            m_viewModel.Cleanup();

            PopupManager.Instance.Exit();

            base.OnExit(_e);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                m_mainWindow.Dispose();
                m_viewModel.Dispose();
            }
        }
    }
}
