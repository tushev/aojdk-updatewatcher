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

namespace AJ_UpdateWatcher
{
    static class SelfUpdate
    {
        const string UserAgent = "aojdk-updatewatcher Auto Updater";
        static string ProductName = $"Update Watcher for {Branding.TargetProduct}";

        static public string LatestVersion_DownloadURL = "";
        static public string LatestVersion_BrowserURL = "";
        static public string LatestVersion_ReleaseName { get; internal set; }
        static public string LatestVersion_ReleaseText { get; internal set; }
        static public string LatestVersion_ReleaseIntroText { get; internal set; }
        static public string LatestVersion_TagName { get; internal set; }
        static public bool Found = false;

        static public bool HasNewVersion(string api)
        {
            Version local_version = Assembly.GetEntryAssembly().GetName().Version;

            // for testing
            // local_version = new Version("0.0.0.0");

            Found = false;

            try
            {
                IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

                HttpClientHandler hch = new HttpClientHandler();
                hch.Proxy = defaultWebProxy;

                //hch.Proxy = ProxyConfigurator.GetWebProxy;
                //hch.UseProxy = ProxyConfigurator.UseProxy;

                var httpClient = new HttpClient(hch);

                httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                var response = httpClient.GetStringAsync(new Uri(api)).Result;
                JObject o = JObject.Parse(response);

                LatestVersion_BrowserURL = (string)o["html_url"];

                LatestVersion_ReleaseName = (string)o["name"];
                LatestVersion_ReleaseText = (string)o["body"];
                LatestVersion_TagName = (string)o["tag_name"];

                string tag = (string)o["tag_name"];
                Version remote_version = new Version(tag);

                LatestVersion_ReleaseIntroText = LatestVersion_ReleaseText.Split(new string[] { "<hr>" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var result = remote_version.CompareTo(local_version);
                if (result > 0)
                {
                    JArray array = (JArray)o["assets"];
                    foreach (var a in array)
                    {
                        string name = (string)a["name"];
                        if (name.IndexOf("setup", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            LatestVersion_DownloadURL = (string)a["browser_download_url"];
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
            if (!Found || LatestVersion_DownloadURL == "") return;
            try
            {
                Uri ur = new Uri(LatestVersion_DownloadURL);

                string tempname = System.IO.Path.GetTempPath() + Guid.NewGuid() + "-" + GetFileNameFromUrl(LatestVersion_DownloadURL);

                IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

                using (WebClient client = new WebClient { Proxy = defaultWebProxy })
                {
                    //client.Proxy = ProxyConfigurator.GetWebProxy;

                    client.DownloadFile(ur, tempname);

                    MessageBox.Show($"Download complete. {Branding.ProductName} will now be closed and the new version will be installed.", ProductName + " :: Updater", MessageBoxButton.OK, MessageBoxImage.Information);
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
        static public void OpenLatestReleaseInBrowser()
        {
            Process.Start(LatestVersion_BrowserURL);
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
