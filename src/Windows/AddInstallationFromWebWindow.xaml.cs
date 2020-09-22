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

        private void Path_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AdoptiumHelpMessagesActions.OpenMainWebPage();
        }

        private void btnWhatHeap_Click(object sender, RoutedEventArgs e)
        {
            AdoptiumHelpMessagesActions.ShowHeapHelp();
        }
    }
}
