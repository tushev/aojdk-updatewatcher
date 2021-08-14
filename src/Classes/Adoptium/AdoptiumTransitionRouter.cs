using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AJ_UpdateWatcher
{
    public class AdoptiumTransitionRouter
    {
        static public bool IsMSIRevisionOnlyUpdate(AdoptiumReleaseVersion a, AdoptiumReleaseVersion b)
        {
            return
                a.Major == b.Major &&
                a.Minor == b.Minor &&
                a.Security == b.Security/* &&
                a.MSIRevision != b.MSIRevision*/
                ;
        }
        static public bool IsAdoptOpenJDK(AdoptiumReleaseVersion a)
        {
            return a.TypeOrEdition == "AdoptOpenJDK";
        }
        static public Tuple<bool,string> SuggestDisabling(Installation i, AdoptiumReleaseVersion just_installed)
        {
            string text = "";

            AdoptiumReleaseVersion previously_installed = i.InstalledVersion;
            var old_file = Path.Combine(previously_installed.LocalPath, "release");

            bool reason_old_autodis_stillpresent = i.IsAutodiscoveredInstance && File.Exists(old_file);
            if (reason_old_autodis_stillpresent)
                text += $"* The old (auto-discovered) installation still seems to be present on the drive: {old_file};" + Environment.NewLine;

            bool reason_old_manual_not_updated = !i.IsAutodiscoveredInstance && File.Exists(old_file);
            if (reason_old_manual_not_updated)
            {
                var u = new Installation(i.Path);
                if (u.Detected)
                {
                    if (just_installed > u.InstalledVersion)
                    {
                        text += $"* The old (manually-added) installation still seems to be present on the drive: version = {u.InstalledVersion.ParsedVersionString}, path = {u.Path}. " + Environment.NewLine;
                        text += $"Most likely, the new installation was installed to somewhere else." + Environment.NewLine;
                    }
                    else
                        reason_old_manual_not_updated = false;
                } else
                {
                    text += $"* The old (manually-added) installation cannot be detected (A VERY RARE CASE, PLEASE MAKE A SCREENSHOT AND CONTACT THE DEVELOPER), but its release file still seems to be present on the drive: {old_file};" + Environment.NewLine;
                }
            }

            bool reason_msi_minor = IsMSIRevisionOnlyUpdate(previously_installed, just_installed) && File.Exists(old_file);
            if (reason_msi_minor)
                text += $"* This is a MSI 'Minor' update: most likely, Windows Installer was unable to uninstall previous version;" + Environment.NewLine;

            bool reason_vendor_changed = IsAdoptOpenJDK(previously_installed) && !IsAdoptOpenJDK(just_installed) && File.Exists(old_file);
            if (reason_vendor_changed)
                text += $"* Vendor change: the new build is produced by [{just_installed.ImplementorRAW.ToUpperInvariant()}], while the installed build is produced by [AdoptOpenJDK]. The old installation still seems to be present on the drive;" + Environment.NewLine;
                        
            //bool reason_old_file_exists = File.Exists(old_file) && i.IsAutodiscoveredInstance;
            //if (reason_old_file_exists)
            //    text += $"* Old installation seems not to be uninstalled: {old_file} still exists" + Environment.NewLine;

            //if is_eol    disable_updates = true; ???

            bool result = reason_old_autodis_stillpresent || reason_old_manual_not_updated || reason_msi_minor || reason_vendor_changed /*|| reason_old_file_exists*/;

            return new Tuple<bool, string>(result, text);
        }
    }
}