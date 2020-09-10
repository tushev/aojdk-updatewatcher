using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adoptium_UpdateWatcher
{
    public class AdoptiumReleaseVersion
    {
        public string Major { get; set; }
        public string Minor { get; set; }
        public string Security { get; set; }
        public string Build { get; set; }
        public string VersionString { get; set; } // like: "openjdk_version": "1.8.0_265-b01" :: version only
        public string ReleaseName { get; set; } // like: "release_name": "jdk-11.0.8+10_openj9-0.21.0" :: extra info
        public string MSIURL { get; set; }
        public string ZIPURL { get; set; }
        public string LocalPath { get; set; }
        public string ImageType { get; set; }

        public bool Found { get; set; }

        public AdoptiumReleaseVersion()
        {
            Found = false;
        }

        public static bool operator > (AdoptiumReleaseVersion a, AdoptiumReleaseVersion b)
        {
            if (Convert.ToInt32(a.Major) > Convert.ToInt32(b.Major)) return true;
            else if (Convert.ToInt32(a.Minor) > Convert.ToInt32(b.Minor)) return true;
            else if (Convert.ToInt32(a.Security) > Convert.ToInt32(b.Security)) return true;
            else if (!String.IsNullOrEmpty(a.Build) && !String.IsNullOrEmpty(b.Build) && Convert.ToInt32(a.Build) > Convert.ToInt32(b.Build)) return true;
            else return false;
        }
        public static bool operator < (AdoptiumReleaseVersion a, AdoptiumReleaseVersion b)
        {
            if (Convert.ToInt32(a.Major) < Convert.ToInt32(b.Major)) return true;
            else if (Convert.ToInt32(a.Minor) < Convert.ToInt32(b.Minor)) return true;
            else if (Convert.ToInt32(a.Security) < Convert.ToInt32(b.Security)) return true;
            else if (!String.IsNullOrEmpty(a.Build) && !String.IsNullOrEmpty(b.Build) && Convert.ToInt32(a.Build) < Convert.ToInt32(b.Build)) return true;
            else return false;
        }
    }
}
