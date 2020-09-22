﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    static public class AdoptiumAPI_ParameterEnumeration
    {
        static public List<string> JVMs = new List<string>() { "hotspot", "openj9" };
        static public List<string> ImageTypes = new List<string>() { "jre", "jdk" };
        static public List<string> HeapSizes = new List<string>() { "normal", "large" };
        static public List<string> Archs = new List<string>() { "x64", "x32" };
    }
    static public class AdoptiumAPI_MostRecentVerbs
    {
        static public readonly string MostRecentVerb = "Most recent";
        static public readonly string MostRecentLTSVerb = "Most recent LTS";
    }

    public class AdoptiumAPI_AvailableReleases
    {
        public List<string> Releases;
        public List<string> LTSReleases;
        public string MostRecentFeatureRelease;
        public string MostRecentLTSRelease;
    }
    static class AdoptiumAPI
    {
        public const string baseDOMAIN = "api.adoptopenjdk.net";
        public const string baseURL = "https://api.adoptopenjdk.net/v3/";

        static public List<string> GetReleases()
        {
            Tuple<List<string>, List<string>> a = GetAllReleases();
            return a.Item1;

        }
        static public Tuple<List<string>, List<string>> GetAllReleases()
        {
            AdoptiumAPI_AvailableReleases all_releases = GetAvailableReleases();

            List<string> releases = all_releases.Releases;
            List<string> LTS_releases = all_releases.LTSReleases;            

            return new Tuple<List<string>, List<string>>( releases, LTS_releases);
        }

        static public AdoptiumAPI_AvailableReleases GetAvailableReleases()
        {
            const string URL = baseURL + "info/available_releases";

            List<string> releases = new List<string>();
            List<string> LTS_releases = new List<string>();
            string MostRecentFeatureRelease = "";
            string MostRecentLTSRelease = "";

            try
            {
                HttpClientHandler hch = new HttpClientHandler();
                hch.Proxy = null;
                hch.UseProxy = false;

                var httpClient = new HttpClient(hch);

                httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));


                var response = httpClient.GetStringAsync(new Uri(URL)).Result;

                JObject joResponse = JObject.Parse(response);

                JArray array = (JArray)joResponse["available_releases"];
                foreach (var a in array)
                    releases.Add(a.Value<string>());

                JArray arrayLTS = (JArray)joResponse["available_lts_releases"];
                foreach (var a in arrayLTS)
                    LTS_releases.Add(a.Value<string>());

                MostRecentFeatureRelease = (string)joResponse["most_recent_feature_release"];
                MostRecentLTSRelease = (string)joResponse["most_recent_lts"];

            }
            catch (Exception ex)
            {
                var ie = ex;
                while (ie.InnerException != null) ie = ie.InnerException;

                var error_message = $"GetAvailableReleases[{URL}]: {ex.Message} => {ie.Message}";
                MessageBox.Show(
                    $"Unable to get list of {Branding.TargetProduct} releases. Make sure you are connected to the internet and {baseDOMAIN} is online." + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                    $"Tech details:{Environment.NewLine}{error_message}"
                    , $"{Branding.TargetProduct} API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            AdoptiumAPI_AvailableReleases info = new AdoptiumAPI_AvailableReleases();
            info.Releases = releases;
            info.LTSReleases = LTS_releases;
            info.MostRecentFeatureRelease = MostRecentFeatureRelease;
            info.MostRecentLTSRelease = MostRecentLTSRelease;

            return info;
        }

        static public AdoptiumReleaseVersion GetLatestVersion(string version, string implementation, string desired_image_type, string desired_heap, out string error_message_out, string desired_arch = "x64", string desired_os = "windows")
        {
            string URL = baseURL + "assets/latest/" + version + "/" + implementation;

            AdoptiumReleaseVersion latest = new AdoptiumReleaseVersion();
            error_message_out = "";

            try
            {
                HttpClientHandler hch = new HttpClientHandler();
                hch.Proxy = null;
                hch.UseProxy = false;

                var httpClient = new HttpClient(hch);
                Debug.WriteLine($"Querying API: {URL}");

                httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                var response = httpClient.GetStringAsync(new Uri(URL)).Result;

                JArray a = JArray.Parse(response);
                //MessageBox.Show(a.ToString());
                foreach (JObject o in a.Children<JObject>())
                {
                    string arch = (string)o["binary"]["architecture"];
                    string image_type = (string)o["binary"]["image_type"];
                    string heap_size = (string)o["binary"]["heap_size"];
                    string os = (string)o["binary"]["os"];

                    if (
                        arch == desired_arch &&
                        image_type == desired_image_type &&
                        os == desired_os &&
                        heap_size == desired_heap
                        )

                    {
                        string version_major = (string)o["version"]["major"];
                        string version_minor = (string)o["version"]["minor"];
                        string version_security = (string)o["version"]["security"];
                        string version_build = (string)o["version"]["build"];
                        string version_string = (string)o["version"]["openjdk_version"];
                        string version_release = (string)o["release_name"];
                        
                        string zip_url = (string)o["binary"]["package"]["link"];

                        string msi_url = (o["binary"]["installer"] != null && o["binary"]["installer"]["link"] != null) ?
                                        (string)o["binary"]["installer"]["link"] : null;

                        //MessageBox.Show(o.ToString());

                        latest.Major = version_major;
                        latest.Minor = version_minor;
                        latest.Security = version_security;
                        latest.Build = version_build;
                        latest.VersionString = version_string;
                        latest.ReleaseName = version_release;

                        latest.MSIURL = msi_url;
                        latest.ZIPURL = zip_url;
                        latest.ImageType = image_type;

                        latest.Found = true;
                        break;
                    }
                }

                if (!latest.Found)
                    error_message_out = "Nothing matches release parameters set.";
            }
            catch (Exception ex)
            {
                var ie = ex;
                while (ie.InnerException != null) ie = ie.InnerException;

                error_message_out += $"GetLatestVersion[{URL}]: {ex.Message}" + (ie.InnerException != null ? $" => {ie.Message}" : "");
                //if (latest.)
                Debug.WriteLine(error_message_out);
                //MessageBox.Show("There was an error: " + ex.Message, "Adoptium API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return latest;
        }
    }
}
