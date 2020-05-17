using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AdoptOpenJDK_UpdateWatcher
{
    static class API
    {
        private const string baseURL = "https://api.adoptopenjdk.net/v3/";



        static public List<string> GetReleases()
        {
            Tuple<List<string>, List<string>> a = GetAllReleases();
            return a.Item1;

        }
        static public Tuple<List<string>, List<string>> GetAllReleases()
        {
            const string URL = baseURL + "info/available_releases";

            List<string> releases = new List<string>();
            List<string> LTS_releases = new List<string>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                    var response = httpClient.GetStringAsync(new Uri(URL)).Result;

                    JObject joResponse = JObject.Parse(response);

                    JArray array = (JArray)joResponse["available_releases"];
                    foreach (var a in array)
                        releases.Add(a.Value<string>());

                    JArray arrayLTS = (JArray)joResponse["available_lts_releases"];
                    foreach (var a in arrayLTS)
                        LTS_releases.Add(a.Value<string>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error: [ " + ex.Message + " ]. Make sure you are connected to the internet and api.adoptopenjdk.net server is online.", "AdoptOpenJDK API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Tuple<List<string>, List<string>>( releases, LTS_releases);
        }

        static public LatestVersion GetLatestVersion(string version, string implementation, string desired_image_type, string desired_heap, string desired_arch = "x64", string desired_os = "windows")
        {
            string URL = baseURL + "assets/latest/" + version + "/" + implementation;

            LatestVersion latest = new LatestVersion();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                    var response = httpClient.GetStringAsync(new Uri(URL)).Result;

                    JArray a = JArray.Parse(response);
                    //MessageBox.Show(a.ToString());
                    foreach (JObject o in a.Children<JObject>())
                    {
                        string arch             = (string)o["binary"]["architecture"];
                        string image_type       = (string)o["binary"]["image_type"];
                        string heap_size        = (string)o["binary"]["heap_size"];
                        string os               = (string)o["binary"]["os"];

                        if (
                            arch == desired_arch &&
                            image_type == desired_image_type &&
                            os == desired_os &&
                            heap_size == desired_heap
                            )

                        {
                            string version_major    = (string)o["version"]["major"];
                            string version_minor    = (string)o["version"]["minor"];
                            string version_security = (string)o["version"]["security"];
                            string version_string   = (string)o["version"]["openjdk_version"];
                            string version_release  = (string)o["release_name"];

                            string msi_url = (string)o["binary"]["installer"]["link"];
                            string zip_url = (string)o["binary"]["package"]["link"];

                            //MessageBox.Show(o.ToString());

                            latest.Major = version_major;
                            latest.Minor = version_minor;
                            latest.Security = version_security;
                            latest.VersionString = version_string;
                            latest.Release = version_release;

                            latest.MSIURL = msi_url;
                            latest.ZIPURL = zip_url;
                            latest.ImageType = image_type;

                            latest.Found = true;
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                
                //MessageBox.Show("There was an error: " + ex.Message, "AdoptOpenJDK API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return latest;
        }
    }
}
