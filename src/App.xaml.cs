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

            bool continue_loading = true;

            continue_loading = HandleSchedulerCommandLine(e, continue_loading);

            if (continue_loading)
            {
                // before doing things, upgrade from previous version
                Upgrade.UpgradeAppSettings();

                // first, check if there is a new version of this tool (if configured so)
                CheckSelfUpdate();

                // load settings
                Machine = AppDataPersistence.TryLoad();

                // prepare updater class
                Updater = new Updater(Machine);
                Updater.RefreshAutoDiscoveredInstancesOnInstallationCompletion = true;

                // add trigger
                AddTriggers();

                ShowDisclaimer();

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
                                Action _ShowNewVersionWindow = () => { ShowNewVersionWindow(false); };

                                /*
                                 * // either - show icon (show delayed)...
                                 * if (Settings.Default.UserConfigurableSetting_UseTrayNotificationForBackgroundCheck)
                                 * {
                                 *     TrayIconUpdatesAreAvailable.UserClickedOnIconOrNotification += (s2, e2) => { _ShowNewVersionWindow(); };                                
                                 *     TrayIconUpdatesAreAvailable.ShowNotification(Machine.StringListOfAvailableUpdates);
                                 * }
                                 * // or show GUI directly
                                 * else
                                 */
                                _ShowNewVersionWindow();
                            }
                            else
                            {
                                // TODO?: move this higher, add notice on ShowConfigurationWindow
                                bool explicit_requested = (e.Args.Length > 0 && e.Args[0] == "-explicitcheck");

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
                                        CheckForFirstRunAndExit(false, $"There were errors while checking for {Branding.TargetProduct} updates. Check your internet connection and ensure {AdoptiumAPI.baseDOMAIN} is reachable.", explicit_requested);
                                }
                                else
                                {
                                    // reset error counter - there were no errors (and no updates)
                                    App.SetUpdateCheckErrorCount(0);

                                    CheckForFirstRunAndExit(false, $"You already have up-to-date version of {Branding.TargetProduct}", explicit_requested);
                                }
                            }
                        };
                        Updater.UpdatesCheckComplete += _app_update_check_complete_eventhandler;

                        Updater.CheckForUpdatesAsync();

                    }
                    else
                    {
                        var ans = MessageBox.Show(
                            $"You don't have any configured {Branding.TargetProduct} installations. Would you like to configure them?",
                            Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (ans == MessageBoxResult.Yes)
                            ShowConfigurationWindow();
                        else
                            Application.Current.Shutdown();

                    } // if-else (Machine.PossiblyHasConfiguredInstallations)

                } //else (!Settings.Default.isConfigured || (e.Args.Length > 0 && e.Args[0] == "-config"))

            } //if (continue_loading)
        }

        private static bool HandleSchedulerCommandLine(StartupEventArgs e, bool continue_loading)
        {
            if (e.Args.Length > 0 && (e.Args[0] == "-deletetask" || e.Args[0] == "-askdeletetask" || e.Args[0] == "-silentlydeletetask"))
            {
                SchedulerManager sm = new SchedulerManager();
                if (sm.TaskIsSet())
                {
                    var result = (e.Args[0] == "-askdeletetask") ?
                        MessageBox.Show($"You have a scheduled task to check for updates of {Branding.TargetProduct}.{Environment.NewLine + Environment.NewLine}Do you want to remove this scheduled task?", Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) :
                        MessageBoxResult.Yes;

                    if (result == MessageBoxResult.Yes)
                    {
                        sm.DeleteTask();

                        if (e.Args[0] != "-silentlydeletetask")
                        {
                            MessageBox.Show("Scheduled task has been removed successfully", Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                continue_loading = false;
                Application.Current.Shutdown();
            }
            
            if (e.Args.Length > 0 && (e.Args[0] == "-forcesettask" || e.Args[0] == "-settask_askifnonconsistent"))
            {
                SchedulerManager sm = new SchedulerManager();
                
                // re-create the task without asking
                if (e.Args[0] == "-forcesettask")
                    sm.ForceReInstallTask();

                // ask if task is present and not-consistent, otherwise create it silently
                if (e.Args[0] == "-settask_askifnonconsistent")
                {
                    if (sm.TaskIsSet())
                        sm.CheckConsistency();
                    else
                        sm.ForceReInstallTask();
                }

                continue_loading = false;
                Application.Current.Shutdown();
            }

            return continue_loading;
        }

        private static void AddTriggers()
        {
            Updater.UpdatesInstallationComplete += (s, _e) =>
            {
                string f = "";
                try
                {
                    if (!String.IsNullOrEmpty(Settings.Default.UserConfigurableSetting_PostUpdateCommand))
                    {
                        f = Settings.Default.UserConfigurableSetting_PostUpdateCommand;
                        Process.Start(f);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"There was an error while executing post-updates-installation command ({f}): {Environment.NewLine + Environment.NewLine}{ex.Message}",
                        Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
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

        private void CheckSelfUpdate()
        {
            if (Settings.Default.CheckForSelfUpdates)
                if (SelfUpdate.HasNewVersion(Settings.Default.SelfUpdatesAPI))
                {
                    var ans = ShowSelfUpdateDialog();

                    //var ans = MessageBox.Show($"A new version of {Branding.ProductName} is available. Would you like to download update (EXE/MSI) and install it?"
                    //    + $"{Environment.NewLine + Environment.NewLine}[Yes] = Download installer (EXE/MSI) and run it"
                    //    + $"{Environment.NewLine}[No] = Do not update now"
                    //    + $"{Environment.NewLine}[Cancel] = Open new release page in default browser (for installer-free ZIPs etc. This app will be closed) "
                    //    + $"{Environment.NewLine + Environment.NewLine}New release name: {SelfUpdate.LatestVersion_ReleaseName}"
                    //    , Branding.MessageBoxHeader, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (ans == MessageBoxResult.Yes)
                    {
                        SelfUpdate.DownloadCloseAndInstallUpdate();
                        System.Windows.Application.Current.Shutdown(); // normally this line should not be triggered
                    }
                    else
                    {
                        if (ans == MessageBoxResult.Cancel)
                        {
                            SelfUpdate.OpenLatestReleaseInBrowser();
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                }
        }

        private MessageBoxResult ShowSelfUpdateDialog()
        {
            var dialog = new AJ_UpdateWatcher.Windows.SelfUpdateDialog();

            this.SetShutdownExplicit();
            bool? ans = dialog.ShowDialog();
            this.SetShutdownOnLastWindowClose();

            return ans == true ? (dialog.OpenReleasePageInstead == false ? MessageBoxResult.Yes : MessageBoxResult.Cancel) : MessageBoxResult.No;
        }

        public void SetShutdownExplicit() { this.ShutdownMode = ShutdownMode.OnExplicitShutdown; }
        public void SetShutdownOnLastWindowClose() { this.ShutdownMode = ShutdownMode.OnLastWindowClose; }

        private static void CheckForFirstRunAndExit(bool invoked_from_ui, string status = "", bool explicit_requested = false)
        {
            if (!invoked_from_ui)
            {
                if (!Settings.Default.FirstSilentRunMessageHasBeenDisplayed || explicit_requested)
                {
                    int maxN = -1;
                    try { maxN = Settings.Default.UserConfigurableSetting_WarnIfNUpdateChecksResultedInErrors; } catch (Exception) { }

                    MessageBox.Show(
                        $"This is the {(explicit_requested ? "explicit message for" : "first background")} check for updates of {Branding.TargetProduct}." + Environment.NewLine + Environment.NewLine +
                        $"It resulted in: [ {status} ]." + Environment.NewLine + Environment.NewLine + 
                        $"{(explicit_requested ? "Normally you should not see a message like this" : "You will receive no such messages in future")}, background checks are completely silent unless there is a new version (or more than {maxN} consequent background errors have occured)." + Environment.NewLine + Environment.NewLine + 
                        $"If you need to change settings, please use [Configure Update Watcher for {Branding.TargetProduct}] shortcut from Start menu.", 
                        Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);

                    if (!explicit_requested)
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

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            TrayIconUpdatesAreAvailable.RemoveTrayIcon();
        }

        private void ShowDisclaimer()
        {
            if (!Settings.Default.NoWarrantiesDisclaimerHasBeenDisplayed)
            {
                var message = $"WARNING! This (independent) software does not GUARANTEE that you will always get " +
                              $"the lastest version of AdoptOpenJDK.{Environment.NewLine + Environment.NewLine}" + 
                              $"Normally, everything works OK, and you get timely updates.{Environment.NewLine + Environment.NewLine}" +
                              $"However, if something breaks or changes in AdoptOpenJDK API, then you may not get the latest version.{Environment.NewLine + Environment.NewLine}" +
                              //$"So, from time to time, it's recommended to check their website to ensure that everything's OK.{Environment.NewLine + Environment.NewLine}" +
                              $"Thus we have to remind you (only once, now):{Environment.NewLine}No warranties provided (see LICENSE), use at your own risk.";

                MessageBox.Show(message, "Disclaimer - " + Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);

                Settings.Default.NoWarrantiesDisclaimerHasBeenDisplayed = true;
                Settings.Default.Save();
            }
        }
    }
}
