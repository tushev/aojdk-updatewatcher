using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AdoptOpenJDK_UpdateWatcher.Properties;

namespace AdoptOpenJDK_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //static public NewVersionWindow NewVersionWindow;
        static public MainWindow MainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CheckForEmptySettings();

            CheckSelfUpdate();

            if (!Settings.Default.isConfigured || (e.Args.Length > 0 && e.Args[0] == "-config"))
            {
                MainWindow = new MainWindow();
                MainWindow.Show();

            }
            else
                CheckForUpdates();
        }

        private void CheckSelfUpdate()
        {
            if (Settings.Default.CheckForSelfUpdates)
                if (SelfUpdate.HasNewVersion(Settings.Default.SelfUpdatesAPI))
                {
                    var ans = MessageBox.Show("New version of AdoptOpenJDK Update Watcher is available. Would you like to download and install it?", "AdoptOpenJDK Update Watcher", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ans == MessageBoxResult.Yes)
                    {
                        SelfUpdate.DownloadCloseAndInstallUpdate();
                        System.Windows.Application.Current.Shutdown(); // normally this line should not be triggered
                    }
                }
        }

        private static void CheckForEmptySettings()
        {

            if (Settings.Default.API_Architecture == "")
            {
                Settings.Default.API_Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x32";
                Settings.Default.Save();
            }
            if (Settings.Default.API_OS == "")
            {
                Settings.Default.API_OS = "windows";
                Settings.Default.Save();
            }
            if (Settings.Default.API_Heap == "")
            {
                Settings.Default.API_Heap = "normal";
                Settings.Default.Save();
            }
        }

        public static void CheckForUpdates(bool invoked_from_ui = false)
        {
            if (LocalInstallation.TryDetect())
            {
                var local_version = LocalInstallation.GetVersion();

                var web_version = GetWebVersion();
                if (web_version.Found)
                {
                    if (web_version.Release != Settings.Default.LastSkippedRelease || invoked_from_ui)
                    {
                        if (web_version > local_version || invoked_from_ui)
                        {
                            //notify + skip suggestion
                            NewVersionWindow NewVersionWindow = new NewVersionWindow(web_version, invoked_from_ui);
                            NewVersionWindow.Show();
                        }
                        else CheckForFirstRunAndExit(invoked_from_ui, "You already have up-to-date version of AdoptOpenJDK");
                    }
                    else CheckForFirstRunAndExit(invoked_from_ui, "You have decided to skip current release of AdoptOpenJDK. Go to Configuration to cancel it");
                    
                }
                else CheckForFirstRunAndExit(invoked_from_ui, "Cannot get latest version of AdoptOpenJDK. Check your internet connection and ensure api.adoptopenjdk.net is online");
            }
            else
            {                
                if (!invoked_from_ui)
                {
                    MessageBox.Show("Cannot detect local instalation of JDK/JRE. Please configure it.", "AdoptOpenJDK Update Watcher", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainWindow = new MainWindow();
                    MainWindow.Show();
                } else
                {
                    LatestVersion web_version = GetWebVersion();
                    if (web_version.Found)
                    {
                        //notify + skip suggestion
                        NewVersionWindow NewVersionWindow = new NewVersionWindow(web_version, invoked_from_ui);
                        NewVersionWindow.Show();
                    } else
                        MessageBox.Show("Cannot find latest version. Make sure you are connected to the internet and api.adoptopenjdk.net server is online.", "AdoptOpenJDK Update Watcher", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private static void CheckForFirstRunAndExit(bool invoked_from_ui, string status = "")
        {
            if (!invoked_from_ui)
            {
                if (Settings.Default.FirstSilentRun)
                {
                    MessageBox.Show("This is the first background check for updates of AdoptOpenJDK. It resulted in: [" + status + "]. You will receive no such messages in future, background checks will be silent unless there is a new version. If you need to change settings, please use [Configure AdoptOpenJDK Update Watcher] shortcut from Start menu.", "AdoptOpenJDK Update Watcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    Settings.Default.FirstSilentRun = false;
                    Settings.Default.Save();
                }
                System.Windows.Application.Current.Shutdown();
            }
        }

        private static LatestVersion GetWebVersion()
        {
            return API.GetLatestVersion(
                Settings.Default.API_MajorVersion,
                Settings.Default.UI_JVM_Implementation == 0 ? "hotspot" : "openj9",
                Settings.Default.UI_JVM_ImageType == 0 ? "jdk" : "jre",
                Settings.Default.API_Heap, Settings.Default.API_Architecture, Settings.Default.API_OS);
        }
    }
}
