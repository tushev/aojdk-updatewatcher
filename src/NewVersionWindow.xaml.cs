using AdoptOpenJDK_UpdateWatcher.Properties;
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

namespace AdoptOpenJDK_UpdateWatcher
{
    /// <summary>
    /// Interaction logic for NewVersionWindow.xaml
    /// </summary>
    public partial class NewVersionWindow : Window
    {
        LatestVersion WebVersion;
        bool invoked_from_ui = false;

        public NewVersionWindow(LatestVersion web_version, bool hide_config_link = false)
        {
            InitializeComponent();

            WebVersion = web_version;
            pbProgress.Visibility = Visibility.Hidden;

            invoked_from_ui = hide_config_link;
            if (invoked_from_ui)
            {
                btnOpenConfig.IsEnabled = false;
                btnSkip.IsEnabled = false;
            }
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            var ans = MessageBox.Show("Are you sure? You will not get notifications until next release will be available.", "AdoptOpenJDK Update Watcher", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ans == MessageBoxResult.Yes)
            {
                Settings.Default.LastSkippedRelease = WebVersion.Release;
                Settings.Default.Save();

                System.Windows.Application.Current.Shutdown();
            }
        }

        private void _this_Loaded(object sender, RoutedEventArgs e)
        {
            txtNewRelease.Text = WebVersion.Release + "\t (image_type: " + WebVersion.ImageType + ")";

            btnDownloadZIP.ToolTip = WebVersion.ZIPURL;
            btnDownloadMSI.ToolTip = WebVersion.MSIURL;
            btnDownloadAndInstallMSI.ToolTip = GetFileNameFromUrl(WebVersion.MSIURL);
            //MessageBox.Show((LocalInstallation.GetVersion() == WebVersion).ToString());

            if (LocalInstallation.Detected && !(WebVersion > LocalInstallation.GetVersion()) && invoked_from_ui)
            {
                lblNewVersionHeader.Foreground = Brushes.LightGray;
                lblNewVersionHeader.TextDecorations.Add(TextDecorations.Strikethrough);
            }
        }

        static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            Uri SomeBaseUri = new Uri("http://canbeanything");

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return System.IO.Path.GetFileName(uri.LocalPath);
        }

        private void btnDownloadAndInstallMSI_Click(object sender, RoutedEventArgs e)
        {
            btnDownloadAndInstallMSI.IsEnabled = false;
            Uri ur = new Uri(WebVersion.MSIURL);
            
            string tempname = System.IO.Path.GetTempPath() + GetFileNameFromUrl(WebVersion.MSIURL);

            pbProgress.Visibility = Visibility.Visible;

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (o, ep) =>
                {
                    Dispatcher.Invoke(() => {
                        pbProgress.Value = ep.ProgressPercentage;
                    });
                };
                client.DownloadFileCompleted += (o, edp) =>
                {
                    DownloadCompleted(tempname);
                };

                client.DownloadFileAsync(ur, tempname);
            }
        }

        private void DownloadCompleted(string tempname)
        {
            Process.Start(tempname);
            System.Windows.Application.Current.Shutdown();
        }

        private void btnDownloadZIP_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebVersion.ZIPURL);
        }

        private void btnDownloadMSI_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebVersion.MSIURL);
        }

        private void btnOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            var m = new MainWindow();
            m.Show();
            this.Close();
        }

        private void _this_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }


    }
}
