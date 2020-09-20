using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace AJ_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for AddInstallationFromWebWindow.xaml
    /// </summary>
    public partial class AddInstallationFromWebWindow : Window
    {
        public AddInstallationFromWebWindow()
        {
            InitializeComponent();
     
            if (App.ConfigurationWindowInstance != null && App.ConfigurationWindowInstance.IsLoaded)
                this.Owner = App.ConfigurationWindowInstance;
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
                "It is an enterprise-grade VM designed for low memory footprint and fast start-up and is used in IBM’s JDK. It is suitable for running all workloads.", "An advice from AdoptOpenJDK.net", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void lblLTS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://adoptopenjdk.net/support.html");
        }

        private void Path_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            Process.Start("https://adoptopenjdk.net/");
        }
    }
}
