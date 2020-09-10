using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Adoptium_UpdateWatcher
{
    static class SelfUpdate
    {
        const string UserAgent = "aojdk-updatewatcher Auto Updater";
        const string ProductName = "Adoptium Update Watcher";

        static public string DownloadURL = "";
        static public bool Found = false;

        static public bool HasNewVersion(string api)
        {
            Version local_version = Assembly.GetEntryAssembly().GetName().Version;
            Found = false;

            try
            {
                HttpClientHandler hch = new HttpClientHandler();
                hch.Proxy = null;
                hch.UseProxy = false;

                var httpClient = new HttpClient(hch);

                httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                var response = httpClient.GetStringAsync(new Uri(api)).Result;
                JObject o = JObject.Parse(response);

                string tag = (string)o["tag_name"];
                Version remote_version = new Version(tag);

                var result = remote_version.CompareTo(local_version);
                if (result > 0)
                {
                    JArray array = (JArray)o["assets"];
                    foreach (var a in array)
                    {
                        string name = (string)a["name"];
                        if (name.IndexOf("setup", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DownloadURL = (string)a["browser_download_url"];
                            Found = true;
                            break;
                        }

                    }
                }

            }
            // better not show anything here as it may interrupt user if checks are being performed as a background task
            catch (Exception ex)
            {
                //MessageBox.Show("There was an error: " + ex.Message, "Adoptium API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Found;
        }
        static public void DownloadCloseAndInstallUpdate()
        {
            if (!Found || DownloadURL == "") return;
            try
            {
                Uri ur = new Uri(DownloadURL);

                string tempname = System.IO.Path.GetTempPath() + Guid.NewGuid() + "-" + GetFileNameFromUrl(DownloadURL);

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(ur, tempname);

                    MessageBox.Show("The application will be closed and new version will be installed.", ProductName + " Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process proc = new Process();
                    proc.StartInfo.FileName = tempname;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();

                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error: " + ex.Message, ProductName + " Updater Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
