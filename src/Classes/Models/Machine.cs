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

namespace AJ_UpdateWatcher
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
            Installations.CollectionChanged += (s, e) => 
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && !HoldInternalReCheckForUpdateSuggested)
                {
                    bool changesContainAtLeastOneCheckedManualElement = false;
                    foreach (Installation i in e.NewItems)
                    {
                        if (i.CheckForUpdatesFlag && !i.IsAutodiscoveredInstance)
                        {
                            //Debug.WriteLine($"changesContainAtLeastOneCheckedManualElement = true for {i.Path}");
                            changesContainAtLeastOneCheckedManualElement = true;
                        }
                    }

                    if (changesContainAtLeastOneCheckedManualElement)
                        SomethingHasBeenChangedSinceUpdateCheck = true;
                }

                MantainNoConflictState(); 
            };
            Installations.ItemPropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Detected")
                    MantainNoConflictState();

                if (e.PropertyName == "CheckForUpdatesFlag" && !HoldInternalReCheckForUpdateSuggested)
                {
                    var i = Installations[e.CollectionIndex];
                    if (i.CheckForUpdatesFlag && i.IsAutodiscoveredInstance)
                        SomethingHasBeenChangedSinceUpdateCheck = true;
                }

                if (
                    e.PropertyName == "WatchedRelease" ||
                    e.PropertyName == "JVM_Implementation" ||
                    e.PropertyName == "ImageType" ||
                    e.PropertyName == "Arch" ||
                    e.PropertyName == "HeapSize"
                    )
                {
                    var i = Installations[e.CollectionIndex];
                    if (i.CheckForUpdatesFlag && !i.IsAutodiscoveredInstance)
                    {
                        SomethingHasBeenChangedSinceUpdateCheck = true;
                    }
                }
            };
           
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

            var _installs = user_scope ? AdoptiumInstallationsDiscoverer.DiscoverInstallationsByRegistryHKCU() :
                                     AdoptiumInstallationsDiscoverer.DiscoverInstallationsByRegistryHKLM();

            HoldAutoReCheckForUpdateSuggested = true;
            for (int k=0; k < _installs.Count; k++)
            {
                DiscoveredInstallation i = _installs[k];

                // we want to reset HoldAutoReCheckForUpdateSuggested at the last element
                if (k == _installs.Count - 1)
                    HoldAutoReCheckForUpdateSuggested = false;

                Installation inst = new Installation(i, true, user_scope);
                Installations.Add(inst);
            }
        }
        public void RefreshAutoDiscoveredInstallations()
        {
            HoldInternalReCheckForUpdateSuggested = true;
            Debug.WriteLine("Refreshing Auto-Discovered Installations...");
            if (DiscoverMachineWideInstallations) { RemoveRegistryAutoDiscoveredInstallations(false); DiscoverAndAddInstallationsFromRegistry(false); }
            if (DiscoverUserScopeInstallations) { RemoveRegistryAutoDiscoveredInstallations(true); DiscoverAndAddInstallationsFromRegistry(true); }
            HoldInternalReCheckForUpdateSuggested = false;
        }
        public bool HoldAutoReCheckForUpdateSuggested = false;
        public bool HoldInternalReCheckForUpdateSuggested = false;

        

        private void MantainNoConflictState()
        {
            // TODO: do not allow duplicates in custom part - postponed ATM, the user is warned when adding new installation
            // can be done with Installations.ItemPropertyChanged

            if (!auto_discovery_activated) return;

            // disable auto-discovered installations that have the same path as one of custom-set installations            
            foreach (var i in Installations)
                if (i.IsAutodiscoveredInstance)
                {
                    var same_pathed = Installations.Where(x => x.Path.TrimEnd('\\') == i.Path.TrimEnd('\\') && x.IsAutodiscoveredInstance == false);
                    if (same_pathed.Count() > 0)
                    {
                        if (i.CheckForUpdatesFlag != false)
                            i.CheckForUpdatesFlag = false;
                    }
                    else
                    {
                        if (i.CheckForUpdatesFlag != true)
                            i.CheckForUpdatesFlag = true;
                    }
                }

            OnPropertyChanged("Installations");
        }

        private bool _something_has_been_changed_since_update_check;
        public bool SomethingHasBeenChangedSinceUpdateCheck 
        {
            get { return _something_has_been_changed_since_update_check; }
            set
            {
                _something_has_been_changed_since_update_check = value;
                OnPropertyChanged("SomethingHasBeenChangedSinceUpdateCheck");
            }
        }

        public void SkipDiscoveredNewVersionForInstallation(Installation i)
        {
            i?.SkipDiscoveredNewVersion();
            //SomethingHasBeenChangedSinceUpdateCheck = true;
        }

        public void RemoveSkippedReleaseForInstallation(Installation i)
        {
            i?.RemoveSkippedRelease();
            SomethingHasBeenChangedSinceUpdateCheck = true;
        }

        public void ResetAllSkippedReleases()
        {
            foreach (var i in Installations)
                i.SkippedReleaseName = null;

            SomethingHasBeenChangedSinceUpdateCheck = true;

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
        private int InstallationsWithSkippedReleasesCount { get { return Installations.Count(x => x.HasSkippedRelease); } }

        

        public bool ThereAreInstallationsWithSkippedReleases { get { return InstallationsWithSkippedReleasesCount > 0; } }
        public int InstallationsWithUpdatesCount { get { return Installations.Count(x => x.HasNewVersion); } }
        public int InstallationsWithMSIUpdatesCount { get { return Installations.Count(x => x.HasMSIInNewVersion); } }
        public string StringListOfAvailableUpdates 
        { 
            get
            { 
                var query = from x in Installations
                            where x.HasNewVersion
                            select x.NewVersion.ParsedVersionString;
                return String.Join(", ", query);
            }
        }

        private bool show_shadowed_installations = false;
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
                return Installations.Count(x => x.Detected && x.CheckForUpdatesFlag) > 0;
            }
        }
        public bool HasConfiguredInstallations
        {
            get
            {
                return Installations.Count(x => String.IsNullOrEmpty(x.Path)) > 0;
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
