using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    class AdoptiumTools
    {
        static public string GetTypeOrEditionString(string id)
        {
            switch (id)
            {
                case "Temurin":
                    return "Eclipse Temurin™";
                case "AdoptOpenJDK":
                    return "AdoptOpenJDK";
                case "SemeruOpen":
                    return "IBM® Semeru® Open Edition";
                default:
                    return "Unknown";
            }

        }

        static public string GetTypeOrEditionFromAdoptiumReleaseVersion(AdoptiumReleaseVersion version)
        {
            if (
                (version.ImplementorRAW ?? "").IndexOf("Temurin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorRAW ?? "").IndexOf("Eclipse Foundation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorVersionRAW ?? "").IndexOf("Temurin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorRAW ?? "").IndexOf("adoptium", StringComparison.OrdinalIgnoreCase) >= 0 
                )
            {
                return "Temurin";
            }
            else if (
                (version.ImplementorRAW ?? "").IndexOf("AdoptOpenJDK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorVersionRAW ?? "").IndexOf("AdoptOpenJDK", StringComparison.OrdinalIgnoreCase) >= 0
                )
            {
                return "AdoptOpenJDK";
            }
            else if (
                (version.ImplementorRAW ?? "").IndexOf("International Business Machines Corporation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorVersionRAW ?? "").IndexOf("International Business Machines Corporation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorRAW ?? "").IndexOf("IBM", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (version.ImplementorVersionRAW ?? "").IndexOf("IBM", StringComparison.OrdinalIgnoreCase) >= 0
                )
            {
                return "SemeruOpen";
            }
            else
            {
                return "";
            }
        }
    }
}
