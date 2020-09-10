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
using Adoptium_UpdateWatcher.Properties;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Configuration;

namespace Adoptium_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        HelpHowToInstallNewWindow HelpHowToInstallNewWindow;
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
                    "Did you forget to configure to check for Eclipse Adoptium JDK/JRE Updates on User Logon?" + Environment.NewLine + Environment.NewLine +
                    "Click Yes to go back and enable this feature," + Environment.NewLine + "No to exit without enabling scheduled tasks.", 
                    Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    gridSchedule.Background = Brushes.LightGoldenrodYellow;
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
            Process.Start("https://github.com/tushev");
        }

        private void btnShowNewVersionWindow_Click(object sender, RoutedEventArgs e)
        {
            App.ShowNewVersionWindow(true);
        }

        private void btnWhatJREJDK_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://stackoverflow.com/questions/1906445/what-is-the-difference-between-jdk-and-jre/#1906455");
        }

        private void btnWhatImpl_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "HotSpot is the VM from the OpenJDK community." + Environment.NewLine +
                "It is the most widely used VM today and is used in Oracle’s JDK. It is suitable for all workloads." + Environment.NewLine + Environment.NewLine +
                "Eclipse OpenJ9 is the VM from the Eclipse community." + Environment.NewLine +
                "It is an enterprise-grade VM designed for low memory footprint and fast start-up and is used in IBM’s JDK. It is suitable for running all workloads.", "An advice from ApdptOpenJDK.net", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void lblLTS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://adoptopenjdk.net/support.html");
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
    }



}
