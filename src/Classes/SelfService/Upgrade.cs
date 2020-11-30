using AJ_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    public static class Upgrade
    {
        public static void UpgradeAppSettings()
        {
            try
            {
                //MessageBox.Show(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                //MessageBox.Show(Settings.Default.SettingsUpgradeRequired.ToString());

                // 1. Upgrade settings
                if (Settings.Default.SettingsUpgradeRequired)
                {
                    // we had "FirstSilentRunMessageHasBeenDisplayed" // "SettingsUpgradeComplete_from_V1" settings in v. 2.0.0.0 release but not in v.1.0.0.0
                    // if at least one is present in previous version - we may upgrade settings
                    bool previousVersionWasAtLeast2000 = false;
                    if (
                        Settings.Default.GetPreviousVersion("FirstSilentRunMessageHasBeenDisplayed") != null ||
                        Settings.Default.GetPreviousVersion("SettingsUpgradeComplete_from_V1") != null
                        )
                        previousVersionWasAtLeast2000 = true;

                    // if previous version was at least 2.0.0.0 - we may proceed
                    if (previousVersionWasAtLeast2000)
                    {

                        // some boilerplate code for future releases
                        /*if (Settings.Default.GetPreviousVersion("SettingsFormatVersion") != null)
                        {
                            string _installedFormatVersion = Settings.Default.GetPreviousVersion("SettingsFormatVersion") as string;
                            Version installedFormatVersion = new Version(_installedFormatVersion);
                        }*/

                        //System.Diagnostics.Debug.WriteLine("Upgrading settings from previous version");
                        //MessageBox.Show("Your settings had been updated from previous version", Branding.MessageBoxHeader);
                        Settings.Default.Upgrade();
                    }

                    // update not required any more
                    Settings.Default.SettingsUpgradeRequired = false;
                    // we must explicitly store current version for it to be available for future upgrades
                    Settings.Default.SettingsFormatVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    // finally, save settings
                    Settings.Default.Save();

                }


                // 2. Upgrade scheduled tasks
                if (!Settings.Default.SettingsUpgradeComplete_from_V1)
                {
                    const string v1TaskName = "AdoptOpenJDK_UpdateWatcher";

                    SchedulerManager sm = new SchedulerManager();

                    if (sm.TaskIsSet(v1TaskName))
                    {
                        if (MessageBox.Show(
                            $"{Branding.ProductName} has detected that you have scheduled 'check for updates' task from version 1." + Environment.NewLine + Environment.NewLine +
                            "It is suggested to remove it and create new task." + Environment.NewLine + Environment.NewLine +
                            "Would you like to do it? [Recommended]",
                            $"Upgrade to v.{Assembly.GetExecutingAssembly().GetName().Version} - {Branding.MessageBoxHeader}", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
                            == MessageBoxResult.Yes)
                        {
                            ExecuteTryUpdateScheduledTask(v1TaskName, sm);
                        }
                        else
                            if (MessageBox.Show(
                            $"In is highly important that the scheduled task will be upgraded. We ask you to reconsider your choice." + Environment.NewLine + Environment.NewLine +
                            "Yes = Please upgrade" + Environment.NewLine +
                            "No  = Keep as is. I'm really sure what I'm doing and I will take full responsibility for upcoming errors.",
                            $"Upgrade to v.{Assembly.GetExecutingAssembly().GetName().Version} - {Branding.MessageBoxHeader}", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)
                            == MessageBoxResult.Yes)
                        {
                            ExecuteTryUpdateScheduledTask(v1TaskName, sm);
                        }
                    }

                    Settings.Default.SettingsUpgradeComplete_from_V1 = true;
                    Settings.Default.Save();

                }
            }
            catch (Exception ex) { MessageBox.Show($"There was an error: [{ex.Message}]", Branding.MessageBoxHeader); };
        }

        private static void ExecuteTryUpdateScheduledTask(string v1TaskName, SchedulerManager sm)
        {
            try
            {
                sm.DeleteTask(v1TaskName);

                if (!sm.TaskIsSet())
                    sm.InstallTask();
                else
                    sm.CheckConsistency();
            }
            catch (Exception ex) { MessageBox.Show($"There was an error: [{ex.Message}]", Branding.MessageBoxHeader); };
        }
    }
}
