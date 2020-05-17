using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoptOpenJDK_UpdateWatcher
{
    public class LatestVersion
    {
        public string Major, Minor, Security, VersionString, Release, MSIURL, ZIPURL, LocalPath, ImageType;
        public bool Found = false;

        public static bool operator > (LatestVersion a, LatestVersion b)
        {
            if (Convert.ToInt32(a.Major) > Convert.ToInt32(b.Major)) return true;
            else if (Convert.ToInt32(a.Minor) > Convert.ToInt32(b.Minor)) return true;
            else if (Convert.ToInt32(a.Security) > Convert.ToInt32(b.Security)) return true;
            else return false;
        }
        public static bool operator < (LatestVersion a, LatestVersion b)
        {
            if (Convert.ToInt32(a.Major) < Convert.ToInt32(b.Major)) return true;
            else if (Convert.ToInt32(a.Minor) < Convert.ToInt32(b.Minor)) return true;
            else if (Convert.ToInt32(a.Security) < Convert.ToInt32(b.Security)) return true;
            else return false;
        }
    }
}
