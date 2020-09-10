using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adoptium_UpdateWatcher.Models.Mockups
{
    public class NewVersionWindowInstallationsMockup
    {
        public ObservableCollection<Installation> InstallationsToUpdate { get; set; }

        public NewVersionWindowInstallationsMockup()
        {
            InstallationsToUpdate.Add(new Installation() { Path = @"C:\Program Files\AdoptOpenJDK\jre-8.0.265.01-hotspot", NewVersion = new AdoptiumReleaseVersion() { ReleaseName = "1.8.0.266" } });
            InstallationsToUpdate.Add(new Installation() { Path = @"C:\Program Files\AdoptOpenJDK\jdk-11.0.8.10-hotspot", NewVersion = new AdoptiumReleaseVersion() { ReleaseName = "11.1.17" } });
            InstallationsToUpdate.Add(new Installation() { Path = @"C:\Program Files\AdoptOpenJDK\jre-11.0.7.10-openj9", NewVersion = new AdoptiumReleaseVersion() { ReleaseName = "11.5.18" } });
        }
    }
}
