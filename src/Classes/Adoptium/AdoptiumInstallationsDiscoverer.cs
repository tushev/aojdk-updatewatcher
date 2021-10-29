using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    public class DiscoveredInstallation
    {
        public string Path;
        public string Version;
        public string JVM_Implementation;
        public string ImageType;
        public string LastElement;
        public bool x64;
    }
    static class AdoptiumInstallationsDiscoverer
    {
        static string[] RegistryRoots = { @"SOFTWARE\AdoptOpenJDK", @"SOFTWARE\Eclipse Foundation", @"SOFTWARE\Eclipse Adoptium", @"SOFTWARE\Temurin", @"SOFTWARE\Semeru" };

        public static List<DiscoveredInstallation> DiscoverInstallationsByRegistryHKLM() { return DiscoverInstallationsByRegistry(RegistryHive.LocalMachine); }
        public static List<DiscoveredInstallation> DiscoverInstallationsByRegistryHKCU() { return DiscoverInstallationsByRegistry(RegistryHive.CurrentUser); }
        public static List<DiscoveredInstallation> DiscoverInstallationsByRegistry(RegistryHive hive)
        {
            List<DiscoveredInstallation> paths = new List<DiscoveredInstallation>();

            var RegistryViews = new List<RegistryView> { RegistryView.Registry32, RegistryView.Registry64 };

            foreach (string AdoptiumRegistryRoot in RegistryRoots)
            {
                foreach (var view in RegistryViews)
                {
                    var key = RegistryKey.OpenBaseKey(hive, view);
                    var level0key = key.OpenSubKey(AdoptiumRegistryRoot);
                    if (level0key != null)
                        foreach (var image in level0key.GetSubKeyNames().AsNotNull())
                            foreach (var version in key.OpenSubKey(AdoptiumRegistryRoot + $@"\{image}").GetSubKeyNames().AsNotNull())
                                foreach (var JVM in key.OpenSubKey(AdoptiumRegistryRoot + $@"\{image}\{version}").GetSubKeyNames().AsNotNull())
                                    foreach (var last_el in key.OpenSubKey(AdoptiumRegistryRoot + $@"\{image}\{version}\{JVM}").GetSubKeyNames().AsNotNull())
                                    {
                                        var path = key.OpenSubKey(AdoptiumRegistryRoot + $@"\{image}\{version}\{JVM}\{last_el}").GetValue("Path");
                                        if (path != null)
                                            if ((string)path != "")
                                                paths.Add(new DiscoveredInstallation()
                                                {
                                                    Path = (string)path,
                                                    Version = version,
                                                    JVM_Implementation = JVM,
                                                    ImageType = image,
                                                    LastElement = last_el,
                                                    x64 = view == RegistryView.Registry64
                                                }); ;
                                    }
                }
            }
            return paths;
        }
    }
}
