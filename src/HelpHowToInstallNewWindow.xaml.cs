using System;
using System.Collections.Generic;
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

namespace Adoptium_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for HelpHowToInstallNewWindow.xaml
    /// </summary>
    public partial class HelpHowToInstallNewWindow : Window
    {
        public HelpHowToInstallNewWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => { this.DragMove(); };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
