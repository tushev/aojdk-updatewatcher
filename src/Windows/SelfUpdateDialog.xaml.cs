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

namespace AJ_UpdateWatcher.Windows
{
    /// <summary>
    /// Interaction logic for SelfUpdateDialog.xaml
    /// </summary>
    public partial class SelfUpdateDialog : Window
    {
        public bool OpenReleasePageInstead = false;

        public SelfUpdateDialog()
        {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOpenRelease_Click(object sender, RoutedEventArgs e)
        {
            OpenReleasePageInstead = true;
            DialogResult = true;
        }
    }
}
