using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        private EventHandler _app_update_check_complete_eventhandler;


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

            if (e.Args.Length > 0 && (e.Args[0] == "-deletetask" || e.Args[0] == "-askdeletetask"))
            {
                SchedulerManager sm = new SchedulerManager();
                if (sm.TaskIsSet())
                {
                    var result = (e.Args[0] == "-askdeletetask") ?
                        MessageBox.Show($"You have a scheduled task to check for updates of {Branding.TargetProduct}.{Environment.NewLine+Environment.NewLine}Do you want to remove this scheduled task?", Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) :
                        MessageBoxResult.Yes;

                    if (result == MessageBoxResult.Yes)
                    {
                        sm.DeleteTask();
                        MessageBox.Show("Scheduled task has been removed successfully", Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                Application.Current.Shutdown();
            }

            // before doing things, upgrade from previous version
            Upgrade.UpgradeAppSettings();

            // first, check if there is a new version of this tool (if configured so)
            CheckSelfUpdate();

            // load settings
            Machine = AppDataPersistence.TryLoad();

            // prepare updater class
            Updater = new Updater(Machine);
            Updater.RefreshAutoDiscoveredInstancesOnInstallationCompletion = true;

            if (!Settings.Default.isConfigured || (e.Args.Length > 0 && e.Args[0] == "-config"))
                ShowConfigurationWindow();
            else
            {
                if (Machine.PossiblyHasConfiguredInstallations)
                {
                    _app_update_check_complete_eventhandler = (s, _e) =>
                    {
                        // remove event handler so this App.xaml.cs-code will never be triggered again from GUI
                        Updater.UpdatesCheckComplete -= _app_update_check_complete_eventhandler;

                        if (Updater.UpdateCheckResultedInNewVersions)
                        {
                            // reset error counter - update is (at least partially) successful
                            App.SetUpdateCheckErrorCount(0);

                            // show GUI
                            ShowNewVersionWindow();                            
                        }
                        else
                        {
                            if (Updater.ErrorsOccuredWhileCheckingForUpdates)
                            {
                                // count errors ...
                                int N = Settings.Default.ErrorsEncounteredSinceLastConfigurationWindowOpened + 1;
                                SetUpdateCheckErrorCount(N);

                                // ... and warn user if N>X
                                if (N >= Settings.Default.UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors)
                                {
                                    var ans = MessageBox.Show(
                                            $"{Branding.ProductName} has encountered {N} sequential errors during background checking for updates (normally, this happens when you login to Windows)." + Environment.NewLine + Environment.NewLine +
                                            $"Most likely it is caused by persistent internet connection issues - {AdoptiumAPI.baseDOMAIN} may be not reachable. Also this could be caused by misconfiguration." + Environment.NewLine + Environment.NewLine +
                                            $"Error counter will be reset now and you will see this message again after {Settings.Default.UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors} new sequential errors." + Environment.NewLine + Environment.NewLine +
                                            //TODO: $"This can be configured in Settings" + Environment.NewLine + Environment.NewLine +
                                            $"Would you like to open Configuration Window for {Branding.ProductName}?"
                                            ,
                                            Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                                    if (ans == MessageBoxResult.Yes)
                                        ShowConfigurationWindow();
                                    else
                                    {
                                        SetUpdateCheckErrorCount(0);
                                        Application.Current.Shutdown();
                                    }
                                }
                                else
                                    CheckForFirstRunAndExit(false, $"There were errors while checking for {Branding.TargetProduct} updates. Check your internet connection and ensure {AdoptiumAPI.baseDOMAIN} is reachable.");
                            }
                            else
                            {
                                // reset error counter - there were no errors (and no updates)
                                App.SetUpdateCheckErrorCount(0);

                                CheckForFirstRunAndExit(false, $"You already have up-to-date version of {Branding.TargetProduct}");
                            }
                        }
                    };
                    Updater.UpdatesCheckComplete += _app_update_check_complete_eventhandler;

                    Updater.CheckForUpdatesAsync();

                } else
                {
                    var ans = MessageBox.Show(
                        $"You don't have any configured {Branding.TargetProduct} installations. Would you like to configure them?", 
                        Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (ans == MessageBoxResult.Yes)
                        ShowConfigurationWindow();
                    else
                        Application.Current.Shutdown();
                }
            }
        }

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

        public static void ShowNewVersionWindow(bool invoked_from_ui = false)
        {
            //if (invoked_from_ui)
            //    Updater.SetToInitialState();

            if (NewVersionWindowInstance == null || NewVersionWindowInstance.IsLoaded == false)
            {
                NewVersionWindowInstance = new NewVersionWindow(true);
                NewVersionWindowInstance.Show();
            }
            else
            {
                NewVersionWindowInstance.Activate();
                NewVersionWindowInstance.RefreshUpdates();
            }

        }

        private void CheckSelfUpdate()
        {
            if (Settings.Default.CheckForSelfUpdates)
                if (SelfUpdate.HasNewVersion(Settings.Default.SelfUpdatesAPI))
                {
                    var ans = MessageBox.Show($"New version of {Branding.ProductName} is available. Would you like to download installer (EXE/MSI) and run it?", Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ans == MessageBoxResult.Yes)
                    {
                        SelfUpdate.DownloadCloseAndInstallUpdate();
                        System.Windows.Application.Current.Shutdown(); // normally this line should not be triggered
                    }
                }
        }

        private static void CheckForFirstRunAndExit(bool invoked_from_ui, string status = "")
        {
            if (!invoked_from_ui)
            {
                if (!Settings.Default.FirstSilentRunMessageHasBeenDisplayed)
                {
                    MessageBox.Show(
                        $"This is the first background check for updates of {Branding.TargetProduct}." + Environment.NewLine + Environment.NewLine +
                        $"It resulted in: [ {status} ]." + Environment.NewLine + Environment.NewLine + 
                        $" You will receive no such messages in future, background checks will be silent unless there is a new version. If you need to change settings, please use [Configure Update Watcher for {Branding.TargetProduct}] shortcut from Start menu.", 
                        Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    Settings.Default.FirstSilentRunMessageHasBeenDisplayed = true;
                    Settings.Default.Save();
                }
                System.Windows.Application.Current.Shutdown();
            }
        }   
        public static void SetUpdateCheckErrorCount(int n)
        {
            Settings.Default.ErrorsEncounteredSinceLastConfigurationWindowOpened = n;
            Settings.Default.Save();
        }
    }
}
