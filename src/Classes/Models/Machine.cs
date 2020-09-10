using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Adoptium_UpdateWatcher
{
    // Machine is the "core" of updater
    // It represents the state of current machine with Adoptium installations there

    // the class is a kind of mix between 'model' and 'viewmodel' ideas.
    public class Machine : ViewModelBase
    {
        // should be turned on when data is loaded
        bool auto_discovery_activated = false;

        private FullyObservableCollection<Installation> installations = new FullyObservableCollection<Installation>();
        public FullyObservableCollection<Installation> Installations
        {
            get { return installations; }
            set
            {
                installations = value;
                OnPropertyChanged("Installations");
            }
        }

        public Machine()
        {
            Installations.CollectionChanged += (s, e) => { MantainNoConflictState(); };            
        }

        
        




       

       

        public void ActivateAutoDiscoveryOnPropertyChange() { auto_discovery_activated = true; }

        private bool discover_machine_wide_installations = true;
        public bool DiscoverMachineWideInstallations { 
            get { return discover_machine_wide_installations; }
            set
            {
                discover_machine_wide_installations = value;
                ToggleAutoDiscoveredInstallationsFromRegistry(value, false);
            }
        }

        private bool discover_user_scope_installations = true;
        public bool DiscoverUserScopeInstallations
        {
            get { return discover_user_scope_installations; }
            set
            {
                discover_user_scope_installations = value;
                ToggleAutoDiscoveredInstallationsFromRegistry(value, true);
            }
        }

        private void ToggleAutoDiscoveredInstallationsFromRegistry(bool new_value, bool user_scope)
        {
            if (!auto_discovery_activated) return;

            if (new_value)
                DiscoverAndAddInstallationsFromRegistry(user_scope);
            else
                RemoveRegistryAutoDiscoveredInstallations(user_scope);

        }

        private void RemoveRegistryAutoDiscoveredInstallations(bool user_scope)
        {
            Installations.Remove(x => (x.IsRegistryAutodiscoveredInstance && x.RegistryUserScope == user_scope));
        }

        private void DiscoverAndAddInstallationsFromRegistry (bool user_scope)
        {
            RemoveRegistryAutoDiscoveredInstallations(user_scope);

            var _installs = user_scope ? AdoptiumInstallationsDetector.DetectInstallationsByRegistryHKCU() :
                                     AdoptiumInstallationsDetector.DetectInstallationsByRegistryHKLM();

            foreach (DetectedInstallation i in _installs)
            {
                Installation inst = new Installation(i, true, user_scope);
                Installations.Add(inst);
            }
        }
        public void RefreshAutoDiscoveredInstallations()
        {
            Debug.WriteLine("Refreshing Auto-Discovered Installations...");
            if (DiscoverMachineWideInstallations) { RemoveRegistryAutoDiscoveredInstallations(false); DiscoverAndAddInstallationsFromRegistry(false); }
            if (DiscoverUserScopeInstallations) { RemoveRegistryAutoDiscoveredInstallations(true); DiscoverAndAddInstallationsFromRegistry(true); }
        }

        

        private void MantainNoConflictState()
        {
            // do not allow duplicates in custom part - postponed ATM, the user is warned when adding new installation

            // disable auto-discovered installations that have the same path as one of custom-set installations            
            foreach (var i in Installations)
                if (i.IsAutodiscoveredInstance)
                {
                    var same_pathed = Installations.Where(x => x.Path.TrimEnd('\\') == i.Path.TrimEnd('\\') && x.IsAutodiscoveredInstance == false);
                    if (same_pathed.Count() > 0)
                        i.CheckForUpdatesFlag = false;
                    else
                        i.CheckForUpdatesFlag = true;
                }

            OnPropertyChanged("Installations");
        }

        private bool _skipped_releases_were_changed_after_update_check;
        public bool SkippedReleasesWereChangedAfterUpdateCheck 
        {
            get { return _skipped_releases_were_changed_after_update_check; }
            set
            {
                _skipped_releases_were_changed_after_update_check = value;
                OnPropertyChanged("SkippedReleasesWereChangedAfterUpdateCheck");
            }
        }

        public void SkipDiscoveredNewVersionForInstallation(Installation i)
        {
            i?.SkipDiscoveredNewVersion();
            //SkippedReleasesWereChangedAfterUpdateCheck = true;
        }

        public void RemoveSkippedReleaseForInstallation(Installation i)
        {
            i?.RemoveSkippedRelease();
            SkippedReleasesWereChangedAfterUpdateCheck = true;
        }

        public void ResetAllSkippedReleases()
        {
            foreach (var i in Installations)
                i.SkippedReleaseName = null;

            SkippedReleasesWereChangedAfterUpdateCheck = true;

            OnPropertyChanged("Installations");
            OnPropertyChanged("ThereAreInstallationsWithSkippedReleases");
        }
        internal void RemoveNotInstalledCheckedInstallations()
        {
            foreach (var i in Installations.ToList())
                if (i.NotInstalled && i.CheckForUpdatesFlag)
                    Installations.Remove(i);

            OnPropertyChanged("Installations");
        }
        private int InstallationsWithSkippedReleasesCount { get { return Installations.Where(x => x.HasSkippedRelease).Count(); } }

        

        public bool ThereAreInstallationsWithSkippedReleases { get { return InstallationsWithSkippedReleasesCount > 0; } }
        public int InstallationsWithUpdatesCount { get { return Installations.Where(x => x.NewVersion != null).Count(); } }

        private bool show_shadowed_installations;
        public bool ShowShadowedInstallations
        {
            get { return show_shadowed_installations; }
            set { 
                show_shadowed_installations = value;
                OnPropertyChanged("ShowShadowedInstallations");
            }
        }

        public bool HasSomeDetectedAndTurnedOnInstallations
        {
            get
            {
                return Installations.Where(x => x.Detected && x.CheckForUpdatesFlag).Count() > 0;
            }
        }
        public bool HasConfiguredInstallations
        {
            get
            {
                return Installations.Where(x => String.IsNullOrEmpty(x.Path)).Count() > 0;
            }
        }
        public bool PossiblyHasConfiguredInstallations
        {
            get
            {
                return DiscoverMachineWideInstallations || DiscoverUserScopeInstallations || HasConfiguredInstallations;
            }
        }



        internal double ComputeTotalUpdateInstallationProgress(bool percent = false)
        {
            const double dl_weight = 0.7;

            double coeff = percent ? 100 : 1;

            double received = installations.Sum(x => x.UpdateBytesReceived);
            double total = installations.Sum(x => x.UpdateTotalBytesToReceive);

            double i = installations.Count(x => x.UpdateInProgress);
            double c = installations.Count(x => x.UpdateIsComplete);
            double s = i + c;

            double q = c > 0 ? 1 : 0;
            double dl = coeff * (total == 0 ? q : (received / total));
            double inst = coeff * (s == 0 ? 0 : (s - i) / s);

            double p = dl * dl_weight + inst * (1 - dl_weight);
            return p;
        }
        internal int ComputeTotalUpdateInstallationProgressInt100()
        {
            return (int)Math.Floor(ComputeTotalUpdateInstallationProgress(true));
        }
    }
}
