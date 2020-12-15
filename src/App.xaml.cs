using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AJ_UpdateWatcher.Properties;

namespace AJ_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public NewVersionWindow NewVersionWindowInstance;
        static public ConfigurationWindow ConfigurationWindowInstance;
        static public AddInstallationFromWebWindow AddInstallationFromWebWindowInstance;
        static public Machine Machine;
        static public Updater Updater;
        static public CommandLineOpts CommandLineOptions;

        static public EventHandler _app_update_check_complete_eventhandler;


        static public AdoptiumAPI_AvailableReleases available_releases;
        static public AdoptiumAPI_AvailableReleases AvailableReleases
        {
            get
            {
                if (available_releases == null)
                    available_releases = AdoptiumAPI.GetAvailableReleases();

                return available_releases;
            }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Initialize(e);
        }

        private void Initialize(StartupEventArgs e)
        {
            CommandLineOptions = CommandLine.Parse(e.Args);

            if (AppStartup.SingleTaskRequested)
            {
                AppStartup.ProcessRequestedSingleTask(this);
                Application.Current.Shutdown();
            }
            else
                AppStartup.InitializeAndRunApp_Main(this);
        }

        public MessageBoxResult ShowSelfUpdateDialog()
        {
            var dialog = new AJ_UpdateWatcher.Windows.SelfUpdateDialog();

            this.SetShutdownExplicit();
            bool? ans = dialog.ShowDialog();
            this.SetShutdownOnLastWindowClose();

            return ans == true ? (dialog.OpenReleasePageInstead == false ? MessageBoxResult.Yes : MessageBoxResult.Cancel) : MessageBoxResult.No;
        }
        public void SetShutdownExplicit() { this.ShutdownMode = ShutdownMode.OnExplicitShutdown; }
        public void SetShutdownOnLastWindowClose() { this.ShutdownMode = ShutdownMode.OnLastWindowClose; }




        public static void ShowAddInstallationFromWebWindow()
        {
            if (AddInstallationFromWebWindowInstance == null || AddInstallationFromWebWindowInstance.IsLoaded == false)
            {
                AddInstallationFromWebWindowInstance = new AddInstallationFromWebWindow();
                AddInstallationFromWebWindowInstance.Show();
            }
            else
                AddInstallationFromWebWindowInstance.Activate();
        }
        
        public static void ShowConfigurationWindow()
        {
            if (ConfigurationWindowInstance == null || ConfigurationWindowInstance.IsLoaded == false)
            {
                ConfigurationWindowInstance = new ConfigurationWindow();
                ConfigurationWindowInstance.Show();
            }
            else
                ConfigurationWindowInstance.Activate();
        }

        public static void ShowNewVersionWindow(bool invoked_from_ui)
        {
            //if (invoked_from_ui)
            //    Updater.SetToInitialState();

            if (NewVersionWindowInstance == null || NewVersionWindowInstance.IsLoaded == false)
            {
                NewVersionWindowInstance = new NewVersionWindow(invoked_from_ui);
                NewVersionWindowInstance.Show();
            }
            else
            {
                NewVersionWindowInstance.SetInvokedFromUIState(invoked_from_ui);
                NewVersionWindowInstance.Activate();
                NewVersionWindowInstance.RefreshUpdates();
            }

        }


        
        public static void SetUpdateCheckErrorCount(int n)
        {
            Settings.Default.ErrorsEncounteredSinceLastConfigurationWindowOpened = n;
            Settings.Default.Save();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            TrayIconUpdatesAreAvailable.RemoveTrayIcon();
        }

    }
}
