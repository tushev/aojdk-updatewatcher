﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AJ_UpdateWatcher
{
    class ConfigurationViewModel : ViewModelBase
    {
        private Machine machine = App.Machine;
        private SchedulerManager schedulerManager = new SchedulerManager();

        public ConfigurationViewModel()
        {
            available_releases = App.AvailableReleases;

            machine.Installations.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged("JavaHomeMessage");
                OnPropertyChanged("CanAddJavaHomeInstance");
                OnPropertyChanged("CannotAddJavaHomeMessage");
                OnPropertyChanged("ThereAreNoInstallations");
                OnPropertyChanged("ThereAreInstallationsWithSkippedReleases"); // maybe better move to content changed event when it will be implemented?
                OnPropertyChanged("CheckForUpdatesButtonText");
            };

            machine.Installations.ItemPropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CheckForUpdatesFlag")
                    OnPropertyChanged("CheckForUpdatesButtonText");

                if (e.PropertyName == "HasSkippedRelease")
                    OnPropertyChanged("ThereAreInstallationsWithSkippedReleases");
            };

            App.Updater.StateChanged += (s, e) => { OnPropertyChanged("CheckForUpdatesButtonText"); };
            App.Updater.UpdatesInstallationComplete += (s, e) => 
            { 
                OnPropertyChanged("JavaHomeMessage");
                OnPropertyChanged("CanAddJavaHomeInstance");
                OnPropertyChanged("CannotAddJavaHomeMessage");
            };

            schedulerManager.CheckConsistency();
        }

        public bool SchedulerManagerTaskStatus
        {
            get { return schedulerManager.TaskStatus; }
            set
            { 
                schedulerManager.TaskStatus = value;
                OnPropertyChanged("SchedulerManagerTaskStatus");
            }
        }

        private Installation _selectedItem;
        public Installation SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");

                RemoveSelectedInstallationCommand.RaiseCanExecuteChanged();
                ConvertToUserOverriddenInstallationCommand.RaiseCanExecuteChanged();
                ResetAPIforSelectedCommand.RaiseCanExecuteChanged();
            }
        }
        public FullyObservableCollection<Installation> ConfiguredInstallations
        {
            get { return machine.Installations; }
            set { 
                machine.Installations = value; 
                OnPropertyChanged("ConfiguredInstallations");
                OnPropertyChanged("ThereAreNoInstallations");
            }
        }
        public bool DiscoverMachineWideInstallations
        {
            get { return machine.DiscoverMachineWideInstallations;  }
            set
            {
                machine.DiscoverMachineWideInstallations = value;
                OnPropertyChanged("DiscoverMachineWideInstallations");
            }
        }
        public bool DiscoverUserScopeInstallations
        {
            get { return machine.DiscoverUserScopeInstallations;  }
            set
            {
                machine.DiscoverUserScopeInstallations = value;
                OnPropertyChanged("DiscoverUserScopeInstallations");
            }
        }
        public bool ShowShadowedInstallations
        {
            get { return machine.ShowShadowedInstallations;  }
            set
            {
                machine.ShowShadowedInstallations = value;
                OnPropertyChanged("ShowShadowedInstallations");
                OnPropertyChanged("ConfiguredInstallations");
            }
        }

        public string LTSReleasesMessage { get { return "LTS Releases: " + string.Join(", ", available_releases.LTSReleases.ToArray()); } }
        public string YourArchMessage { get { return "Yours: " + (Environment.Is64BitOperatingSystem ? "x64" : "x32"); } }

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

        public bool ThereAreInstallationsWithSkippedReleases { get { return machine.ThereAreInstallationsWithSkippedReleases; } }
        public bool ThereAreNoInstallations { get { return machine.Installations.Count == 0; } }
        public string CheckForUpdatesButtonText
        {
            get
            {
                return 
                   App.Updater.SomethingInProgress ? "Open updater window" :
                                   (machine.Installations.Where(i => i.NotInstalled && i.CheckForUpdatesFlag).Count() == 0 ?
                                   $"Check for {Branding.TargetProduct} updates" :
                                   $"Install new {Branding.TargetProduct} releases and update existing ones");
            }
        }


        public void SaveModel()
        {
            AppDataPersistence.Save(machine);
        }

        public string JavaHomeMessage
        {
            get
            {
                var java_home = Environment.GetEnvironmentVariable("JAVA_HOME");
                if (!String.IsNullOrEmpty(java_home))
                    return java_home;
                // TODO: improve
                //return LocalInstallation.CheckPath(java_home, false) ? java_home : "[!] No JDK/JRE found in " + java_home;
                else return "JAVA_HOME is not set";
            }
        }

        public bool CanAddJavaHomeInstance
        {
            get
            {
                var java_home = (Environment.GetEnvironmentVariable("JAVA_HOME") ?? "").TrimEnd('\\');

                if (java_home == "")
                    return false;

                foreach (Installation i in ConfiguredInstallations)
                    if (i.IsJavaHomeInstance /*|| (i.IsAutodiscoveredInstance && i.Path.TrimEnd('\\') == java_home)*/)
                        return false;

                return true;
            }
        }
        public string CannotAddJavaHomeMessage
        {
            get
            {
                return String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAVA_HOME")) ? 
                    "JAVA__HOME is not set" :
                    "JAVA__HOME is already in the list";
            }
        }

        #region AddJAVA_HOMEInstallationCommand
        ICommand add_javahome_installation_command;
        public ICommand AddJAVA_HOMEInstallationCommand
        {
            get
            {
                if (add_javahome_installation_command == null)
                {
                    add_javahome_installation_command = new DelegateCommand(CanExecuteAddJAVA_HOMEInstallationCommand, ExecuteAddJAVA_HOMEInstallationCommand);
                }
                return add_javahome_installation_command;
            }
        }
        private void ExecuteAddJAVA_HOMEInstallationCommand(object parameter)
        {
            bool add = true;

            try
            {
                if (!Properties.Settings.Default.JavaHomeWarningHasBeenDisplayed &&
                    (machine.DiscoverMachineWideInstallations || machine.DiscoverUserScopeInstallations)
                   )
                { 
                    var ans = MessageBox.Show(
                                 "Generally, you don't need adding JAVA_HOME, if you have auto-discovery turned on. Auto-discovery will monitor all your installations for updates." + Environment.NewLine + Environment.NewLine +
                                 "Add JAVA_HOME **only** if you need to override something, or to switch to Latest (LTS) feature branch, or if your know what are you doing." + Environment.NewLine + Environment.NewLine +
                                 "Proceed with adding JAVA_HOME? ",
                                 Branding.ProductName, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    if (ans == MessageBoxResult.No)
                        add = false;

                    Properties.Settings.Default.JavaHomeWarningHasBeenDisplayed = true;
                }
            }
            catch (Exception) { }

            if (add)
            {
                Installation new_installation = new Installation(true);
                ConfiguredInstallations.Add(new_installation);
            }
        }
        public bool CanExecuteAddJAVA_HOMEInstallationCommand(object parameter) { return true; }
        #endregion

        
        private string BrowseAndCheckForPath(ObservableCollection<Installation> installations)
        {
            using (var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    string path = dialog.FileName;
                    var same_pathed = installations.Where(x => x.Path.TrimEnd('\\') == path.TrimEnd('\\') && x.IsAutodiscoveredInstance == false);
                    if (same_pathed.Count() > 0)
                    {
                        var ans = MessageBox.Show(
                            $"You already have [{path}] in your user-added list. Do you really want to add it again [not recommended]?. Press No if you are not sure what to do.",
                            Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);

                        if (ans == MessageBoxResult.No)
                            return null;
                    }

                    if ( !(new Installation(path)).Detected)
                    {
                        var ans = MessageBox.Show(
                            $"Cannot detect any {Branding.TargetProduct} installation in [{path}]. Do you really want to add this path? (No = Select another, Cancel = Add nothing)",
                            Branding.MessageBoxHeader, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.No);

                        if (ans == MessageBoxResult.Cancel)
                            return null;
                        else if (ans == MessageBoxResult.No)
                            return BrowseAndCheckForPath(installations);
                    }

                    return path;
                }
            }

            return null;
        }


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
            var path = BrowseAndCheckForPath(ConfiguredInstallations);
            if (path != null)
            {
                Installation new_installation = new Installation(path);
                ConfiguredInstallations.Add(new_installation);
            }
        }
        private bool CanExecuteAddInstallationCommand(object parameter) { return true; }
        #endregion       

        #region ResetAPIforSelectedCommand
        DelegateCommand reset_API_for_selected_command;
        public DelegateCommand ResetAPIforSelectedCommand
        {
            get
            {
                if (reset_API_for_selected_command == null)
                {
                    reset_API_for_selected_command = new DelegateCommand(CanExecuteResetAPIforSelectedCommand, ExecuteResetAPIforSelectedCommand);
                }
                return reset_API_for_selected_command;
            }
        }
        private void ExecuteResetAPIforSelectedCommand(object parameter)
        {
            if (null != SelectedItem)
            {
                SelectedItem.SetReleaseParametersFromPath();
            }
        }
        private bool CanExecuteResetAPIforSelectedCommand(object parameter) { return SelectedItem != null && !SelectedItem.IsAutodiscoveredInstance && !SelectedItem.NotInstalled; }
        #endregion

        #region RemoveSelectedInstallationCommand
        DelegateCommand remove_selected_installation_command;
        public DelegateCommand RemoveSelectedInstallationCommand
        {
            get
            {
                if (remove_selected_installation_command == null)
                {
                    remove_selected_installation_command = new DelegateCommand(CanExecuteRemoveSelectedInstallationCommand, ExecuteRemoveSelectedInstallationCommand);
                }
                return remove_selected_installation_command;
            }
        }
        private void ExecuteRemoveSelectedInstallationCommand(object parameter)
        {
            if (null != SelectedItem)
            {
                bool is_java_home = SelectedItem.IsJavaHomeInstance;
                string itemToRemoveName = (is_java_home ? "JAVA_HOME instance" : "[" + SelectedItem.DisplayPath + "]");

                string question = SelectedItem.OverridesAutodiscovered ?
                    $"Remove user override for {itemToRemoveName}?{Environment.NewLine+Environment.NewLine}API params will be reset to defaults, and an auto-discovered instance should appear in the list." :
                    $"Remove {itemToRemoveName} from the list of monitored installations?";

                var result =
                    String.IsNullOrEmpty(SelectedItem.Path) ? MessageBoxResult.Yes : 
                    MessageBox.Show(question, "Confirm removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ConfiguredInstallations.Remove(SelectedItem);
                }
            }
        }
        private bool CanExecuteRemoveSelectedInstallationCommand(object parameter) { return !(SelectedItem == null || SelectedItem.IsAutodiscoveredInstance); }
        #endregion

        #region ConvertToUserOverriddenInstallationCommand
        DelegateCommand convert_to_user_overridden_installation_command;
        public DelegateCommand ConvertToUserOverriddenInstallationCommand
        {
            get
            {
                if (convert_to_user_overridden_installation_command == null)
                {
                    convert_to_user_overridden_installation_command = new DelegateCommand(CanExecuteConvertToUserOverriddenInstallationCommand, ExecuteConvertToUserOverriddenInstallationCommand);
                }
                return convert_to_user_overridden_installation_command;
            }
        }
        private void ExecuteConvertToUserOverriddenInstallationCommand(object parameter)
        {
            if (null != SelectedItem)
            {
                if (SelectedItem.Path != null)
                {
                    //// TODO: DRY code: this is a duplicate
                    Installation new_installation = new Installation(SelectedItem.Path);

                    bool disable_updates = ((string)parameter??"").ToLowerInvariant() == "disableupdatescheck";
                    if (disable_updates)
                        new_installation.CheckForUpdatesFlag = false;

                    ConfiguredInstallations.Add(new_installation);
                }
            }
        }
        private bool CanExecuteConvertToUserOverriddenInstallationCommand(object parameter) { return (SelectedItem != null && SelectedItem.IsAutodiscoveredInstance && SelectedItem.CheckForUpdatesFlag); }
        #endregion

        #region OpenPathInExplorerCommand
        ICommand open_path_in_explorer_command;
        public ICommand OpenPathInExplorerCommand
        {
            get
            {
                if (open_path_in_explorer_command == null)
                {
                    open_path_in_explorer_command = new DelegateCommand(CanExecuteOpenPathInExplorerCommand, ExecuteOpenPathInExplorerCommand);
                }
                return open_path_in_explorer_command;
            }
        }

        private void ExecuteOpenPathInExplorerCommand(object parameter)
        {
            if (null != SelectedItem)
            {
                var path = SelectedItem.Path ?? "";
                if (System.IO.Directory.Exists(path))
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer", path);
                    }
                    catch (Exception) { }
                }
            }
        }
        private bool CanExecuteOpenPathInExplorerCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ResetAllSkippedReleasesCommand
        ICommand reset_all_skipped_releases_command;
        public ICommand ResetAllSkippedReleasesCommand
        {
            get
            {
                if (reset_all_skipped_releases_command == null)
                {
                    reset_all_skipped_releases_command = new DelegateCommand(CanExecuteResetAllSkippedReleasesCommand, ExecuteResetAllSkippedReleasesCommand);
                }
                return reset_all_skipped_releases_command;
            }
        }
        private void ExecuteResetAllSkippedReleasesCommand(object parameter)
        {
            var result = MessageBox.Show("Are you sure?", "Confirm resetting all skipped releases", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                machine.ResetAllSkippedReleases();

            OnPropertyChanged("ThereAreInstallationsWithSkippedReleases");
        }
        private bool CanExecuteResetAllSkippedReleasesCommand(object parameter) { return true; }
        #endregion

        #region BrowsePathCommand
        ICommand browse_path_command;
        public ICommand BrowsePathCommand
        {
            get
            {
                if (browse_path_command == null)
                {
                    browse_path_command = new DelegateCommand(CanExecuteBrowsePathCommand, ExecuteBrowsePathCommand);
                }
                return browse_path_command;
            }
        }

        private void ExecuteBrowsePathCommand(object parameter)
        {
            var path = BrowseAndCheckForPath(ConfiguredInstallations);
            if (path != null)
            {
                (parameter as Installation).DisplayPath = path;
            }
        }
        private bool CanExecuteBrowsePathCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region EditTaskCommand
        ICommand edit_task_command;
        public ICommand EditTaskCommand
        {
            get
            {
                if (edit_task_command == null)
                {
                    edit_task_command = new DelegateCommand(CanExecuteEditTaskCommand, ExecuteEditTaskCommand);
                }
                return edit_task_command;
            }
        }
        private void ExecuteEditTaskCommand(object parameter) { schedulerManager.EditTask(); }
        private bool CanExecuteEditTaskCommand(object parameter) { return true; }

        #endregion

        #region AddInstallationFromWebCommand
        ICommand add_installation_from_web_command;
        public ICommand AddInstallationFromWebCommand
        {
            get
            {
                if (add_installation_from_web_command == null)
                {
                    add_installation_from_web_command = new DelegateCommand(CanExecuteAddInstallationFromWebCommand, ExecuteAddInstallationFromWebCommand);
                }
                return add_installation_from_web_command;
            }
        }
        private void ExecuteAddInstallationFromWebCommand(object parameter) { App.ShowAddInstallationFromWebWindow(); }
        private bool CanExecuteAddInstallationFromWebCommand(object parameter) { return true; }

        #endregion

        public string TargetProduct { get { return Branding.TargetProduct; } }
        public string ProductName { get { return Branding.ProductName; } }
        
    }
}
