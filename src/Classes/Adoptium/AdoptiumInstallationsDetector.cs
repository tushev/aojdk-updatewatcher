using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adoptium_UpdateWatcher
{
    public class DetectedInstallation
    {
        public string Path;
        public string Version;
        public string JVM_Implementation;
        public string ImageType;
        public string LastElement;
        public bool x64;
    }
    static class AdoptiumInstallationsDetector
    {
        static string AdoptiumRegistryRoot = @"SOFTWARE\AdoptOpenJDK";

        public static List<DetectedInstallation> DetectInstallationsByRegistryHKLM() { return DetectInstallationsByRegistry(RegistryHive.LocalMachine); }
        public static List<DetectedInstallation> DetectInstallationsByRegistryHKCU() { return DetectInstallationsByRegistry(RegistryHive.CurrentUser); }
        public static List<DetectedInstallation> DetectInstallationsByRegistry(RegistryHive hive)
        {
            List<DetectedInstallation> paths = new List<DetectedInstallation>();

            var RegistryViews = new List<RegistryView> { RegistryView.Registry32, RegistryView.Registry64 };
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
                                            paths.Add(new DetectedInstallation() 
                                            { 
                                                Path = (string)path, Version = version, 
                                                JVM_Implementation = JVM, 
                                                ImageType = image, 
                                                LastElement = last_el,
                                                x64 = view == RegistryView.Registry64
                                            });;
                                }
            }
            return paths;
        }
    }
}
