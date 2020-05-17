using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using AdoptOpenJDK_UpdateWatcher.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Configuration;

namespace AdoptOpenJDK_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string taskName = "AdoptOpenJDK_UpdateWatcher";

        public MainWindow()
        {
            InitializeComponent();
                        
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();

            if (!Settings.Default.isConfigured && (bool)cbSchedule.IsChecked == false)
            {
                var ans = MessageBox.Show("Are you sure you do not want to configure AdoptOpenJDK Update Watcher to check for AdoptOpenJDK Updates on User login? Click No to go back and enable this feature.", "AdoptOpenJDK Update Watcher", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ans == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    gridSchedule.Background = Brushes.LightGoldenrodYellow;
                    return;
                }
            }
            if (!Settings.Default.isConfigured)
            {
                if (
                    lstReleases.SelectedItem != null &&
                    lstImplementation.SelectedItem != null &&
                    lstImageType.SelectedItem != null &&
                    ((bool)useJavaHome.IsChecked || (bool)useLocalPath.IsChecked) &&
                    LocalInstallation.Detected
                    )
                {
                    Settings.Default.isConfigured = true;
                    Settings.Default.Save();
                }
            }
            
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    txtLocalPath.Text = dialog.FileName;
                    CheckProvidedLocalPath();
                }
            }
        }

        static public string GetJavaHomeMessage()
        {
            var java_home = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!String.IsNullOrEmpty(java_home))
                return LocalInstallation.CheckPath(java_home, false) ? java_home : "[!] No JDK/JRE found in " + java_home;
            else return "JAVA_HOME is not set";
        }


        private void CheckProvidedLocalPath()
        {
            try
            {
                if (txtLocalPath != null)
                { 
                    var path = txtLocalPath.Text;

                    if (Directory.Exists(path))
                    {
                        if (LocalInstallation.CheckPath(path, useLocalPath.IsChecked.Value))
                            Settings.Default.LocalInstallationPath = path;
                        else
                            MessageBox.Show("This directory does not contain JDK/JRE. Previous value will be restored upon app restart.", path, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                        MessageBox.Show("Specified directory does not exist. Previous value will be restored upon app restart.", path, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            } catch (Exception ex) { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var releases = API.GetAllReleases();
            lstReleases.ItemsSource = releases.Item1;
            lblLTS.Content = "LTS Releases: " + string.Join(", ", releases.Item2.ToArray());
            lblJAVA_HOME.Text = GetJavaHomeMessage();

            btnCancelSkippedRelease.IsEnabled = Settings.Default.LastSkippedRelease.Length > 0;

            lblExtraParams.Content = "arch: " + Settings.Default.API_Architecture + "; os: " + Settings.Default.API_OS + "; heap__size: " + Settings.Default.API_Heap;

            if (useJavaHome.IsChecked.Value)
                LocalInstallation.CheckJavaHome();
            else
            {
                string path = Properties.Settings.Default.LocalInstallationPath;
                if (Directory.Exists(path))
                    LocalInstallation.CheckPath(path);
            } 

            if (txtLocalPath.Text == "")
                txtLocalPath.Text = Environment.GetEnvironmentVariable("JAVA_HOME");

            // make tips visible or hidden (more convenient to develop)
            if (!Settings.Default.isConfigured && String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAVA_HOME")) && lstReleases.Items.Count > 0)
            {
                lblInstallTip.Visibility = Visibility.Visible;
                imgInstallTipArrow.Visibility = Visibility.Visible;

                lblExtraParams.Foreground = Brushes.Gray;
            }
            else
            {
                lblInstallTip.Visibility = Visibility.Hidden;
                imgInstallTipArrow.Visibility = Visibility.Hidden;
            }
        }

        private void txtLocalPath_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckProvidedLocalPath();
        }

        private void txtLocalPath_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key != System.Windows.Input.Key.Enter) return;

            // your event handler here
            e.Handled = true;
            CheckProvidedLocalPath();
        }

        private void useJavaHome_Checked(object sender, RoutedEventArgs e)
        {
            LocalInstallation.CheckJavaHome();
        }

        private void useLocalPath_Checked(object sender, RoutedEventArgs e)
        {
            CheckProvidedLocalPath();
        }

        private void cbSchedule_Checked(object sender, RoutedEventArgs e)
        {
            
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = "Checks for updates of AdoptOpenJDK";
            td.Principal.LogonType = TaskLogonType.InteractiveToken;

            // V2 only: Add a delayed logon trigger for a specific user
            LogonTrigger lt2 = td.Triggers.Add(new LogonTrigger { UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name });
            lt2.Delay = TimeSpan.FromMinutes(1);

            // Add an action that will launch Notepad whenever the trigger fires
            td.Actions.Add(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            // Register the task in the root folder            
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, td);
        }

        private void cbSchedule_Unchecked(object sender, RoutedEventArgs e)
        {
            // Remove the task we just created
            TaskService.Instance.RootFolder.DeleteTask(taskName);
        }

        private void btnEditTask_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.TaskScheduler.Task t = TaskService.Instance.GetTask(taskName);
            if (t == null) return;

            TaskEditDialog editorForm = new TaskEditDialog(t, true, true);
            editorForm.ShowDialog();
        }

        private void btnEditEnvironmentVariables_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "rundll32.exe";
                proc.StartInfo.Arguments = "sysdm.cpl,EditEnvironmentVariables";
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch (Exception ex)
            {
                Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");
            }
            
        }

        private void lblCopyright_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/tushev");
        }

        private void btnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            App.CheckForUpdates(true);
        }

        private void btnCancelSkippedRelease_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Release " + Settings.Default.LastSkippedRelease + " has been re-enabled.", "AdoptOpenJDK Update Watcher", MessageBoxButton.OK, MessageBoxImage.Information);
            Settings.Default.LastSkippedRelease = "";
            btnCancelSkippedRelease.IsEnabled = false;
        }

        private void btnWhatJREJDK_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://stackoverflow.com/questions/1906445/what-is-the-difference-between-jdk-and-jre/#1906455");
        }

        private void btnWhatImpl_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("HotSpot is the VM from the OpenJDK community. It is the most widely used VM today and is used in Oracle’s JDK. It is suitable for all workloads. Eclipse OpenJ9 is the VM from the Eclipse community. It is an enterprise-grade VM designed for low memory footprint and fast start-up and is used in IBM’s JDK. It is suitable for running all workloads.", "An advice from ApdptOpenJDK.net", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void lblLTS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://adoptopenjdk.net/support.html");
        }

        private void lblExtraParams_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                MessageBox.Show("These values are pre-defined. Do not change them until absolutely necessary. CTRL+SHIFT+ALT + Double-right-click it to open editor :).", "AdoptOpenJDK Update Watcher");

        }

        private void lblExtraParams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && 
                (Keyboard.Modifiers & ModifierKeys.Control ) > 0 &&
                (Keyboard.Modifiers & ModifierKeys.Shift) > 0 &&
                (Keyboard.Modifiers & ModifierKeys.Alt) > 0
                )
            {
                var ans = MessageBox.Show("These values are pre-defined. Do not change them unless absolutely necessary. If you know what you are doing, press YES, otherwise press NO.", "AdoptOpenJDK Update Watcher", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ans == MessageBoxResult.Yes)
                {
                    var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = "notepad++";
                        proc.StartInfo.Arguments = path;
                        proc.Start();
                    }
                    catch (Exception ex)
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = "notepad.exe";
                        proc.StartInfo.Arguments = path;
                        proc.Start();                        
                    }
                }
                
            }
        }
    }



}
