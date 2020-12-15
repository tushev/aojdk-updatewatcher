using AJ_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    static class AppStartup
    {
        static public bool SingleTaskRequested
        {
            get
            {
                return SchedulerCommandLineTasks.HasSingleTask; // || SomethingOther.HasSingleTask
            }
        }

        internal static void ProcessRequestedSingleTask(App app)
        {
            if (SchedulerCommandLineTasks.HasSingleTask)
                SchedulerCommandLineTasks.ProcessSingleTask();
        }

        public static void InitializeApp()
        {
            // load settings
            App.Machine = AppDataPersistence.TryLoad();

            // prepare updater class
            App.Updater = new Updater(App.Machine);
            App.Updater.RefreshAutoDiscoveredInstancesOnInstallationCompletion = true;

            // add triggers
            AddTriggers();
        }


        public static void InitializeAndRunApp_Main(App app)
        {
            // before doing things, upgrade from previous version
            Upgrade.UpgradeAppSettings();

            // first, check if there is a new version of this tool (if configured so)
            CheckSelfUpdate(app);

            // initialize App object, load configuration, add generic triggers etc
            InitializeApp();

            ShowDisclaimer();

            if (!Settings.Default.isConfigured || App.CommandLineOptions.OpenConfigurationWindow)
                App.ShowConfigurationWindow();
            else
            {
                if (App.Machine.PossiblyHasConfiguredInstallations)
                {
                    if (App.CommandLineOptions.RunExplicitCheckForUpdates)
                        App.ShowNewVersionWindow(true);
                    else
                        PerformBackgroundUpdateCheck();
                }
                else
                {
                    var ans = MessageBox.Show(
                        $"You don't have any configured {Branding.TargetProduct} installations. Would you like to configure them?",
                        Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (ans == MessageBoxResult.Yes)
                        App.ShowConfigurationWindow();
                    else
                        Application.Current.Shutdown();

                } // if-else (Machine.PossiblyHasConfiguredInstallations)

            } //else (!Settings.Default.isConfigured || App.CommandLineOptions.OpenConfigurationWindow)


        }

        private static void PerformBackgroundUpdateCheck()
        {
            App._app_update_check_complete_eventhandler = (s, _e) =>
            {
                // remove event handler so this App.xaml.cs-code will never be triggered again from GUI
                App.Updater.UpdatesCheckComplete -= App._app_update_check_complete_eventhandler;

                if (App.Updater.UpdateCheckResultedInNewVersions)
                {
                    // reset error counter - update is (at least partially) successful
                    App.SetUpdateCheckErrorCount(0);

                    // show GUI
                    Action _ShowNewVersionWindow = () => { App.ShowNewVersionWindow(false); };

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
                    if (App.Updater.ErrorsOccuredWhileCheckingForUpdates)
                    {
                        // count errors ...
                        int N = Settings.Default.ErrorsEncounteredSinceLastConfigurationWindowOpened + 1;
                        App.SetUpdateCheckErrorCount(N);

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
                                App.ShowConfigurationWindow();
                            else
                            {
                                App.SetUpdateCheckErrorCount(0);
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
            App.Updater.UpdatesCheckComplete += App._app_update_check_complete_eventhandler;

            App.Updater.CheckForUpdatesAsync();
        }
       

        private static void AddTriggers()
        {
            App.Updater.UpdatesInstallationComplete += (s, _e) =>
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


        private static void ShowDisclaimer()
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

        private static void CheckSelfUpdate(App app)
        {
            if (Settings.Default.CheckForSelfUpdates)
                if (SelfUpdate.HasNewVersion(Settings.Default.SelfUpdatesAPI))
                {
                    var ans = app.ShowSelfUpdateDialog();

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
    }
}
