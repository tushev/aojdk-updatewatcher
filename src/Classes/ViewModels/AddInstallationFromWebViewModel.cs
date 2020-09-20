using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AJ_UpdateWatcher
{
    class AddInstallationFromWebViewModel : ViewModelBase
    {
        private Machine machine = App.Machine;
        private Installation newItem;

        public AddInstallationFromWebViewModel()
        {
            available_releases = App.AvailableReleases;
            NewItem = new Installation();

            DownloadImmediately = true;
        }

        
        
        public Installation NewItem
        {
            get { return newItem; }
            set
            {
                newItem = value;
                OnPropertyChanged("NewItem");
            }
        }
        public FullyObservableCollection<Installation> InstallationsList
        {
            get { return machine.Installations; }
            set { 
                machine.Installations = value; 
                OnPropertyChanged("InstallationsList");
            }
        }

        public bool DownloadImmediately { get; set; }
       
        public string LTSReleasesMessage { get { return "LTS Releases: " + string.Join(", ", available_releases.LTSReleases.ToArray()); } }
        public string YourArchMessage { get { return "Your system: " + (Environment.Is64BitOperatingSystem ? "x64" : "x32"); } }

        private AdoptiumAPI_AvailableReleases available_releases;
        public ObservableCollection<string> AvailableReleases
        {
            // TODO: move to API (?)
            get
            {
                ObservableCollection<string> list = new ObservableCollection<string>() { AdoptiumAPI_MostRecentVerbs.MostRecentVerb, AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb };
                foreach (var el in available_releases.Releases)
                    list.Add(el);
                return list;
            }
        }
        public List<string> JVMs { get { return AdoptiumAPI_ParameterEnumeration.JVMs; } }
        public List<string> ImageTypes { get { return AdoptiumAPI_ParameterEnumeration.ImageTypes; } }
        public List<string> HeapSizes { get { return AdoptiumAPI_ParameterEnumeration.HeapSizes; } }
        public List<string> Archs { get { return AdoptiumAPI_ParameterEnumeration.Archs; } }        


        #region AddInstallationCommand
        ICommand add_installation_command;
        public ICommand AddInstallationCommand
        {
            get
            {
                if (add_installation_command == null)
                {
                    add_installation_command = new DelegateCommand(CanExecuteAddInstallationCommand, ExecuteAddInstallationCommand);
                }
                return add_installation_command;
            }
        }
        private void ExecuteAddInstallationCommand(object parameter)
        {
            var installationToAdd = newItem;
            NewItem = new Installation();

            installationToAdd.CheckForUpdatesFlag = true;
            InstallationsList.Add(installationToAdd);

            if (DownloadImmediately)
                App.ShowNewVersionWindow(true);

            App.AddInstallationFromWebWindowInstance.Close();
        }
        private bool CanExecuteAddInstallationCommand(object parameter) { return true; }
        #endregion       

        public string TargetProduct { get { return Branding.TargetProduct; } }
        public string ProductName { get { return Branding.ProductName; } }
        
    }
}
