using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace AJ_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btnChoosePostInstallCommand_Click(object sender, RoutedEventArgs e)
        {
            bool oldTopMost = this.Topmost;
            this.Topmost = false;

            using (var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                dialog.Filters.Add(new CommonFileDialogFilter("Executable files", "*.exe; *.com; *.bat; *.cmd; *.ps1; *.vbs; *.sh; *.ps2; *.jar"));
                dialog.Filters.Add(new CommonFileDialogFilter("All files", "*.*"));


                Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    txtPostInstallCommand.Text = dialog.FileName;
                }
            }

            this.Topmost = oldTopMost;
        }
    }
}
