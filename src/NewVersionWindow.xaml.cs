using AJ_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
using System.Windows.Threading;

namespace AJ_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for NewVersionWindow.xaml
    /// </summary>
    public partial class NewVersionWindow : Window
    {
        NewVersionViewModel NewVersionVM;

        public NewVersionWindow(bool hide_config_link = false)
        {
            InitializeComponent();

            NewVersionVM = new NewVersionViewModel(hide_config_link);
            this.DataContext = NewVersionVM;

            HeaderArea.MouseLeftButtonDown += (s, e) => { this.DragMove(); };
        }

        public void RefreshUpdates()
        {
            NewVersionVM.ForceUpdateCheck();
        }

        private void btnOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            App.ShowConfigurationWindow();
        }

        private void NewVersionWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NewVersionVM.SaveModel();
            Settings.Default.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
