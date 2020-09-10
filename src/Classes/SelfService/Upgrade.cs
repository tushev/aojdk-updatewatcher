using Adoptium_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Adoptium_UpdateWatcher
{
    public static class Upgrade
    {
        public static void UpgradeAppSettings()
        {
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
                        try
                        {
                            sm.DeleteTask(v1TaskName);
                            sm.InstallTask();
                        }
                        catch (Exception ex) { MessageBox.Show($"There was an error: [{ex.Message}]", Branding.MessageBoxHeader); };
                    }
                }

                Settings.Default.SettingsUpgradeComplete_from_V1 = true;
                Settings.Default.Save();
            }
        }
    }
}
