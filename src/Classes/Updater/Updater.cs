using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adoptium_UpdateWatcher
{
    public enum UpdaterState
    {
        UpdateCheckNotPerformed,
        UpdateCheckInProgress,
        UpdateCheckComplete,
        UpdateInstallationInProgress,
        UpdateInstallationComplete
    }
    public class Updater : ViewModelBase
    {
        private UpdateChecker updateChecker = new UpdateChecker();
        private UpdateInstaller updateInstaller = new UpdateInstaller();

        private UpdaterState _state;
        private bool updatecheck_queued = false;

        public EventHandler InstallerProgressChanged;
        public EventHandler StateChanged;

        public event EventHandler UpdatesCheckComplete;
        public event EventHandler UpdatesInstallationStarted;
        public event EventHandler UpdatesInstallationComplete;

        public List<string> ErrorsEncounteredWhileCheckingForUpdates;

        private Machine machine;
        public Updater(Machine _parent)
        {
            machine = _parent;
            SetToInitialState();

            updateChecker.ThereAreNewVersions += (s, _e) => { UpdateCheckResultedInNewVersions = true; };
            updateChecker.NoNewVersions += (s, _e) => { UpdateCheckResultedInNewVersions = false; };
            updateChecker.ErrorsOccuredWhileChecking += (s, _e) =>
            {
                ErrorsEncounteredWhileCheckingForUpdates = updateChecker.ErrorsEncountered;
                ErrorsOccuredWhileCheckingForUpdates = true;
            };
            updateChecker.CheckComplete += (s, _e) =>
            {
                State = UpdaterState.UpdateCheckComplete;
                UpdatesCheckComplete?.Invoke(this, _e);

                if (updatecheck_queued)
                {
                    updatecheck_queued = false;
                    Debug.WriteLine("updateChecker.CheckComplete: running queued CheckForUpdatesAsync");
                    CheckForUpdatesAsync();
                };
            };

            updateInstaller.UpdateProcessStarted += (s, _e) =>
            {
                State = UpdaterState.UpdateInstallationInProgress;
                UpdatesInstallationStarted?.Invoke(this, _e); 
            };
            updateInstaller.ProgressChanged += (s, _e) => { InstallerProgressChanged?.Invoke(s, _e); };
            updateInstaller.ErrorsOccuredWhileUpdating += (s, _e) => { ErrorsOccuredWhileInstallingUpdates = true; };
            updateInstaller.UpdateProcessCompleted += (s, _e) =>
            {
                State = UpdaterState.UpdateInstallationComplete;
                UpdatesInstallationComplete?.Invoke(this, _e);
            };
        }

        public void SetToInitialState()
        {
            ErrorsEncounteredWhileCheckingForUpdates = null;
            State = UpdaterState.UpdateCheckNotPerformed;
        }

        public void CheckForUpdatesAsync()
        {
            if (_state == UpdaterState.UpdateCheckInProgress)
            {
                updatecheck_queued = true;
                Debug.WriteLine("Updater.CheckForUpdatesAsync: UpdateCheckInProgress => Queued");
                return;
            }


            Debug.WriteLine("Started Updater.CheckForUpdatesAsync...");
            SetToInitialState();
            State = UpdaterState.UpdateCheckInProgress;

            updateChecker.CheckAsync(machine);
        }
        public void DownloadAndInstallUpdatesAsync()
        {            
            updateInstaller.DownloadAndInstallUpdatesAsync(machine);
        }

        public UpdaterState State 
        {
            get { return _state; } 
            private set
            {
                _state = value;

                if (_state == UpdaterState.UpdateCheckNotPerformed) 
                { 
                    UpdateCheckPerformed = false;
                    UpdateCheckResultedInNewVersions = false;
                    ErrorsOccuredWhileCheckingForUpdates = false;
                    UpdateInstallationComplete = false;
                    ErrorsOccuredWhileInstallingUpdates = false;
                }
                if (_state == UpdaterState.UpdateCheckComplete)
                {
                    machine.SomethingHasBeenChangedSinceUpdateCheck = false;
                    UpdateCheckPerformed = true;
                }
                if (_state == UpdaterState.UpdateInstallationComplete) 
                { 
                    UpdateInstallationComplete = true;

                    machine.RemoveNotInstalledCheckedInstallations();

                    if (RefreshAutoDiscoveredInstancesOnInstallationCompletion)
                        machine.RefreshAutoDiscoveredInstallations();
                }

                OnPropertyChanged("State");

                OnPropertyChanged("SomethingInProgress");
                OnPropertyChanged("UpdateCheckInProgress");
                OnPropertyChanged("UpdateInstallationInProgress");

                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool RefreshAutoDiscoveredInstancesOnInstallationCompletion { get; set; }

        #region Progress State-reflecting Properties
        public bool SomethingInProgress { 
            get { return State == UpdaterState.UpdateCheckInProgress || State == UpdaterState.UpdateInstallationInProgress; } 
        }
        public bool UpdateCheckInProgress
        { 
            get { return State == UpdaterState.UpdateCheckInProgress; } 
        }
        public bool UpdateInstallationInProgress
        { 
            get { return State == UpdaterState.UpdateInstallationInProgress; } 
        }
        #endregion

        #region Update Checking Properties
        private bool _update_check_performed = false;
        public bool UpdateCheckPerformed { 
            get { return _update_check_performed; }
            private set
            {
                _update_check_performed = value;
                OnPropertyChanged("UpdateCheckPerformed");
                OnPropertyChanged("AllInstallationsAreUpToDate");
            }
        }

        private bool _there_are_new_versions = false;
        public bool UpdateCheckResultedInNewVersions
        {
            get { return _there_are_new_versions; }
            private set
            {
                _there_are_new_versions = value;
                OnPropertyChanged("UpdateCheckResultedInNewVersions");
                OnPropertyChanged("AllInstallationsAreUpToDate");
            }
        }

        private bool _errors_occured_while_checking_for_updates = false;
        public bool ErrorsOccuredWhileCheckingForUpdates
        {
            get { return _errors_occured_while_checking_for_updates; }
            private set
            {
                _errors_occured_while_checking_for_updates = value;
                OnPropertyChanged("ErrorsOccuredWhileCheckingForUpdates");
            }
        }
        #endregion

        #region Update Installation Properties
        private bool _update_installation_complete = false;
        public bool UpdateInstallationComplete
        {
            get { return _update_installation_complete; }
            private set
            {
                _update_installation_complete = value;
                OnPropertyChanged("UpdateInstallationComplete");
                OnPropertyChanged("AllInstallationsAreUpToDate");
            }
        }

        private bool _errors_occured_while_installing_updates = false;
        public bool ErrorsOccuredWhileInstallingUpdates
        {
            get { return _errors_occured_while_installing_updates; }
            private set
            {
                _errors_occured_while_installing_updates = value;
                OnPropertyChanged("ErrorsOccuredWhileInstallingUpdates");
            }
        }
        #endregion

        public bool AllInstallationsAreUpToDate
        {
            get { return (UpdateCheckPerformed && !UpdateCheckResultedInNewVersions) || UpdateInstallationComplete; }
        }
        
    }
}
