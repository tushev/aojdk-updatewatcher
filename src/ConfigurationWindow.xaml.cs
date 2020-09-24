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
using AJ_UpdateWatcher.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Configuration;

namespace AJ_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        HelpHowToInstallNewWindow HelpHowToInstallNewWindow;
        SettingsWindow SettingsWindowInstance;

        ConfigurationViewModel ConfigurationVM = new ConfigurationViewModel();

        public ConfigurationWindow()
        {
            InitializeComponent();
            this.DataContext = ConfigurationVM;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigurationVM.SaveModel();

            Settings.Default.Save();

            if (!Settings.Default.isConfigured && (bool)cbSchedule.IsChecked == false)
            {
                var ans = MessageBox.Show(
                    $"Did you forget to configure to check for {Branding.TargetProduct} JDK/JRE Updates on User Logon?" + Environment.NewLine + Environment.NewLine +
                    "Yes = go back and enable this feature [Recommended]" + Environment.NewLine + "No = exit without enabling scheduled update check.", 
                    Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    gridSchedule.Background = /*gridSchedule.Background == Brushes.LightGoldenrodYellow ? */
                                              Brushes.Yellow /*: Brushes.LightGoldenrodYellow*/;
                    return;
                }
            }
            if (!Settings.Default.isConfigured)
            {
                if ( App.Machine.PossiblyHasConfiguredInstallations )
                {
                    Settings.Default.isConfigured = true;
                    Settings.Default.Save();
                }
            }
            
        }
        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void DataGridCell_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lookup for the source to be DataGridCell
                if (e.OriginalSource.GetType() == typeof(DataGridCell) &&
                    !((e.OriginalSource as DataGridCell).Column.DisplayIndex == 0 ||
                       (sender as DataGrid).CurrentCell.Column.DisplayIndex == 0)
                     )
                {
                    (sender as DataGrid).BeginEdit(e);
                }
            }
            catch (Exception) { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.SetUpdateCheckErrorCount(0);

            lstReleasesInGrid.ItemsSource = ConfigurationVM.AvailableReleases;
            lstJVMInGrid.ItemsSource = ConfigurationVM.JVMs;
            lstImageTypeInGrid.ItemsSource = ConfigurationVM.ImageTypes;
            lstArchInGrid.ItemsSource = ConfigurationVM.Archs;
            lstHeapSizeInGrid.ItemsSource = ConfigurationVM.HeapSizes;

            if (!Settings.Default.isConfigured && (bool)cbSchedule.IsChecked == false)
                gridSchedule.Background = Brushes.LightGoldenrodYellow;

            //if (!Settings.Default.isConfigured)
                //lblGrayedOut.Foreground = Brushes.IndianRed;

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
            catch (Exception)
            {
                Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");
            }
            
        }

        private void lblCopyright_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Process.Start("https://github.com/tushev");
        }

        private void btnShowNewVersionWindow_Click(object sender, RoutedEventArgs e)
        {
            App.ShowNewVersionWindow(true);
        }

        private void btnWhatJREJDK_Click(object sender, RoutedEventArgs e)
        {
            AdoptiumHelpMessagesActions.ShowWhatJREJDKHelp();
        }

        private void btnWhatImpl_Click(object sender, RoutedEventArgs e)
        {
            AdoptiumHelpMessagesActions.ShowJVM_ImplementationHelp();
        }

        private void lblLTS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AdoptiumHelpMessagesActions.ShowLTSHelp();
        }       

        private void btnStarOnGithub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tushev/aojdk-updatewatcher/stargazers");
        }

        private void btnOpenHelpHowToInstallNewWindow_Click(object sender, RoutedEventArgs e)
        {
            if (HelpHowToInstallNewWindow == null || HelpHowToInstallNewWindow.IsLoaded == false)
            {
                HelpHowToInstallNewWindow = new HelpHowToInstallNewWindow();
                HelpHowToInstallNewWindow.Show();
            }
            else
                HelpHowToInstallNewWindow.Activate();
        }

        private void btnWhatHeap_Click(object sender, RoutedEventArgs e)
        {
            AdoptiumHelpMessagesActions.ShowHeapHelp();
        }

        private void cbSchedule_Checked(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.isConfigured)
                gridSchedule.Background = Brushes.Transparent;
        }

        private void btnOpenHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tushev/aojdk-updatewatcher/wiki");
        }

        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsWindowInstance == null || SettingsWindowInstance.IsLoaded == false)
            {
                SettingsWindowInstance = new SettingsWindow();
                SettingsWindowInstance.Owner = this;
                SettingsWindowInstance.Show();
            }
            else
                SettingsWindowInstance.Activate();
        }


        private void btnHowEditGrayedOut_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tushev/aojdk-updatewatcher/wiki/Types-of-installations");
        }
    }



}
