using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Adoptium_UpdateWatcher
{
    class NewVersionViewModel : ViewModelBase
    {
        private Machine machine = App.Machine;
        private Updater updater = App.Updater;
        private bool showAllEnabledInstallations;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public bool InvokedFromUI { get; set; }

        public NewVersionViewModel(bool invoked_from_ui = false)
        {
            InvokedFromUI = invoked_from_ui;
            ShowAllEnabledInstallations = false;

            if (!updater.UpdateCheckPerformed)
                updater.CheckForUpdatesAsync();

            InstallationsToUpdate.CollectionChanged += (s, e) => 
            {
                if (!updater.UpdateInstallationComplete && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    bool changesContainAtLeastOneDetectedElement = false;
                    foreach (Installation i in e.NewItems)
                    {
                        if (i.Detected)
                        {
                            //Debug.WriteLine($"changesContainAtLeastOneDetectedElement = true for {i.Path}");
                            changesContainAtLeastOneDetectedElement = true;
                        }
                    }

                    if (changesContainAtLeastOneDetectedElement)
                    { 
                        //Debug.WriteLine($"Starting update from InstallationsToUpdate.CollectionChanged += (s, e) => .. because changesContainAtLeastOneDetectedElement = true");
                        ForceUpdateCheck();
                    }
                    else
                        RefreshUI();
                }
                else
                    RefreshUI();                
            };
            InstallationsToUpdate.ItemPropertyChanged += (s, e) => 
            {
                //Debug.WriteLine($"Item = {e.CollectionIndex}, PropertyName = {e.PropertyName}");
                if (e.PropertyName == "CheckForUpdatesFlag")
                {
                    var i = InstallationsToUpdate[e.CollectionIndex];
                    //Debug.WriteLine($"i.Path = {i.Path}");

                    if (i.CheckForUpdatesFlag && !i.IsAutodiscoveredInstance)
                    {
                        //Debug.WriteLine($"Starting update because ItemPropertyChanged => CheckForUpdatesFlag==true for {i.Path} [i.IsAutodiscoveredInstance={i.IsAutodiscoveredInstance}]");
                        ForceUpdateCheck();
                    }
                    else
                        RefreshUI();
                }
            };

            updater.StateChanged += (s, _e) => { RefreshUI(); };
            updater.UpdatesInstallationStarted += (s, _e) => { dispatcherTimer.Start(); };
            updater.UpdatesInstallationComplete += (s, _e) => { dispatcherTimer.Stop(); };
            updater.InstallerProgressChanged += (s, _e) => 
            {
                var ea = _e as AdoptiumUpdateInstallerEventArgs;
                if (ea.PercentChanged)
                {

                }
                else
                {
                    RefreshUI();
                }
            };            

            installationsToUpdateSource = new CollectionViewSource();
            installationsToUpdateSource.Source = InstallationsToUpdate;
            installationsToUpdateSource.Filter += InstallationsToUpdateFilter;

            machine.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SkippedReleasesWereChangedAfterUpdateCheck")
                    OnPropertyChanged("ShowThereMayBeNewVersionsMessage");
            };

            dispatcherTimer.Tick += (s, e) =>
            {
                if (updater.UpdateInstallationInProgress)
                {
                    OnPropertyChanged("TotalProgress1");
                    OnPropertyChanged("TotalProgress100");
                }
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            
        }



        public void ForceUpdateCheck()
        {
            updater.CheckForUpdatesAsync();
        }


        #region InstallationsToUpdate
        public FullyObservableCollection<Installation> InstallationsToUpdate
        {
            get { return machine.Installations; }
            set
            {
                machine.Installations = value;
                OnPropertyChanged("InstallationsToUpdate");
            }
        }
        internal CollectionViewSource installationsToUpdateSource { get; set; }
        public ICollectionView InstallationsToUpdateSourceCollection
        {
            get { return installationsToUpdateSource.View; }
        }
        private void InstallationsToUpdateFilter(object sender, FilterEventArgs e)
        {
            var x = (Installation)e.Item;

            bool hasNewVersion = ShowAllEnabledInstallations ?  true : x.NewVersion != null;
            bool isSetAndOn = /*!String.IsNullOrEmpty(x.Path) &&*/ x.CheckForUpdatesFlag;
            bool show = isSetAndOn && hasNewVersion;

            e.Accepted = show;
        }
        private void OnFilterChanged()
        {
            installationsToUpdateSource?.View?.Refresh();
        }
        #endregion



        private void RefreshUI()
        {
            OnPropertyChanged("InstallationsToUpdate");
            OnPropertyChanged("NewVersionsAvailableMessage");
            OnPropertyChanged("ShowNewVersionsAvailableMessage");
            OnPropertyChanged("ShowUIInstallationsToUpdate");
            OnPropertyChanged("ShowUpdateCheckInProgressMessage");
            OnPropertyChanged("ShowErrorsOccuredWhileCheckingForUpdatesMessage");
            OnPropertyChanged("ErrorsEncounteredWhileCheckingForUpdatesString");
            OnPropertyChanged("ShowThereMayBeNewVersionsMessage");
            OnPropertyChanged("ShowAllInstallationsAreUpToDateMessage");
            OnPropertyChanged("IsButtonDownloadAndInstallUpdatesEnabled");
            OnPropertyChanged("SomethingInProgress");
            OnPropertyChanged("TaskbarItemProgressState");
            OnPropertyChanged("UpdateInstallationInProgress");
            OnFilterChanged();
        }

        public int TotalProgress100 { get { return machine.ComputeTotalUpdateInstallationProgressInt100(); } }
        public double TotalProgress1 { get { return machine.ComputeTotalUpdateInstallationProgress(); } }

        public TaskbarItemProgressState TaskbarItemProgressState
        {
            get
            {
                return SomethingInProgress ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.None;
            }
        }


        #region Local GUI - checkboxes etc.
        public bool ShowAllEnabledInstallations 
        { 
            get => showAllEnabledInstallations;
            set
            {
                showAllEnabledInstallations = value; 
                OnFilterChanged();
                OnPropertyChanged("ShowAllInstallationsAreUpToDateMessage");
                OnPropertyChanged("ShowThereMayBeNewVersionsMessage");
            }
        }
        #endregion

        #region Local GUI - Messages and Enabled/Visibility flags
        public string NewVersionsAvailableMessage { get { return InstallationsWithUpdatesCount > 1 ? "New Versions are available!" : "New Version is available!"; } }
        public bool ShowNewVersionsAvailableMessage { get { return InstallationsWithUpdatesCount > 0; } }

        public bool ShowUIInstallationsToUpdate
        {
            get { return updater.UpdateCheckPerformed; }
        }

        public bool ShowUpdateCheckInProgressMessage
        {
            get { return updater.UpdateCheckInProgress; }
        }
        public bool ShowErrorsOccuredWhileCheckingForUpdatesMessage
        {
            get { return updater.State == UpdaterState.UpdateCheckComplete && updater.ErrorsOccuredWhileCheckingForUpdates; }
        }
        public string ErrorsEncounteredWhileCheckingForUpdatesString
        {
            get { return string.Join(Environment.NewLine, updater.ErrorsEncounteredWhileCheckingForUpdates ?? new List<string>()); }
        }

        public bool ShowThereMayBeNewVersionsMessage
        {
            get { return !SomethingInProgress && machine.SkippedReleasesWereChangedAfterUpdateCheck; }
        }
        public bool ShowAllInstallationsAreUpToDateMessage
        {
            get
            {
                return (updater.AllInstallationsAreUpToDate || InstallationsWithUpdatesCount == 0) &&
                        !ShowAllEnabledInstallations && !SomethingInProgress && !machine.SkippedReleasesWereChangedAfterUpdateCheck;
            }
        }
        public bool IsButtonDownloadAndInstallUpdatesEnabled
        {
            get { return !SomethingInProgress && InstallationsWithUpdatesCount > 0; }
        }
        public bool SomethingInProgress
        {
            get { return updater.SomethingInProgress; }
        }
        public bool UpdateInstallationInProgress
        {
            get { return updater.UpdateInstallationInProgress; }
        }
        #endregion

        private int InstallationsWithUpdatesCount { get { return machine.InstallationsWithUpdatesCount; } }
        
        public void SaveModel()
        {
            AppDataPersistence.Save(machine);
        }

        


        #region OpenReleaseURLCommand
        ICommand open_release_URL_command;
        public ICommand OpenReleaseURLCommand
        {
            get
            {
                open_release_URL_command = open_release_URL_command ?? new DelegateCommand(CanExecuteOpenReleaseURLCommand, ExecuteOpenReleaseURLCommand);
                return open_release_URL_command;
            }
        }
        private void ExecuteOpenReleaseURLCommand(object parameter)
        {
            try
            {
                string key = (((Tuple<object, object>)parameter).Item2 as string ?? "").ToLowerInvariant();
                var i = ((Tuple<object, object>)parameter).Item1 as Installation;

                if (i?.NewVersion != null)
                {
                    if (key == "zip")
                        Process.Start(i.NewVersion.ZIPURL);
                    if (key == "msi")
                        Process.Start(i.NewVersion.MSIURL);
                    if (key == "zip2cb")
                        Clipboard.SetText(i.NewVersion.ZIPURL);
                    if (key == "msi2cb")
                        Clipboard.SetText(i.NewVersion.MSIURL);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while executing ExecuteOpenReleaseURLCommand: {ex.Message}");
            }
        }
        public bool CanExecuteOpenReleaseURLCommand(object parameter)
        {
            return true;
            // var i = ((Tuple<object, object>)parameter)?.Item1 as Installation;
            // return i?.HasNewVersion ?? false;
        }
        #endregion

        #region SkipReleaseCommand / ContextMenu
        ICommand skip_release_command;
        public ICommand SkipReleaseCommand
        {
            get
            {
                skip_release_command = skip_release_command ?? new DelegateCommand(CanExecuteSkipReleaseCommand, ExecuteSkipReleaseCommand);
                return skip_release_command;
            }
        }
        private void ExecuteSkipReleaseCommand(object parameter)
        {
            machine.SkipDiscoveredNewVersionForInstallation((parameter as Installation));
            RefreshUI();
        }
        public bool CanExecuteSkipReleaseCommand(object parameter) { return true; }
        #endregion

        #region RemoveSkippedReleaseCommand / ContextMenu
        ICommand remove_skipped_release_command;
        public ICommand RemoveSkippedReleaseCommand
        {
            get
            {
                remove_skipped_release_command = remove_skipped_release_command ?? new DelegateCommand(CanExecuteRemoveSkippedReleaseCommand, ExecuteRemoveSkippedReleaseCommand);
                return remove_skipped_release_command;
            }
        }
        private void ExecuteRemoveSkippedReleaseCommand(object parameter)
        {
            machine.RemoveSkippedReleaseForInstallation((parameter as Installation));
            RefreshUI();
        }
        public bool CanExecuteRemoveSkippedReleaseCommand(object parameter) { return true; }
        #endregion
        
        #region RefreshUpdatesCommand
        ICommand refresh_updates_command;
        public ICommand RefreshUpdatesCommand
        {
            get
            {
                refresh_updates_command = refresh_updates_command ?? new DelegateCommand(CanExecuteRefreshUpdatesCommand, ExecuteRefreshUpdatesCommand);
                return refresh_updates_command;
            }
        }
        private void ExecuteRefreshUpdatesCommand(object parameter) { ForceUpdateCheck(); }
        public bool CanExecuteRefreshUpdatesCommand(object parameter) { return true; }
        #endregion
        
        #region DownloadAndInstallUpdatesCommand
        ICommand download_and_install_updates_command;
        public ICommand DownloadAndInstallUpdatesCommand
        {
            get
            {
                download_and_install_updates_command = download_and_install_updates_command ?? new DelegateCommand(CanExecuteDownloadAndInstallUpdatesCommand, ExecuteDownloadAndInstallUpdatesCommand);
                return download_and_install_updates_command;
            }
        }
        private void ExecuteDownloadAndInstallUpdatesCommand(object parameter)
        {
            updater.DownloadAndInstallUpdatesAsync();
        }
        public bool CanExecuteDownloadAndInstallUpdatesCommand(object parameter) { return true; }
        #endregion
    }
}
