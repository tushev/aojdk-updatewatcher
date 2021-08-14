using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    public class UpdateChecker
    {
        private bool there_are_new_versions;
        private bool errors_occured;

        public List<string> ErrorsEncountered;

        public EventHandler CheckComplete;
        public EventHandler ThereAreNewVersions;
        public EventHandler NoNewVersions;
        public EventHandler ErrorsOccuredWhileChecking;

        public UpdateChecker()
        {
            ResetFlags();
        }

        private void ResetFlags()
        {
            ErrorsEncountered = new List<string>();

            there_are_new_versions = false;
            errors_occured = false;
        }

        public void CheckAsync(Machine machine, bool force = false)
        {
            ResetFlags();
            BackgroundWorker worker = new BackgroundWorker();

            worker.WorkerReportsProgress = false;
            there_are_new_versions = false;

            worker.DoWork += (s, e) =>
            {
                try
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    foreach (Installation i in machine.Installations)
                    {
                        if (!i.CheckForUpdatesFlag || i.UpdateInProgress)
                            continue;

                        i.NewVersion = null;

                        var release = i.WatchedRelease;

                        // check for verbs
                        if (release == AdoptiumAPI_MostRecentVerbs.MostRecentVerb)
                            release = App.AvailableReleases.MostRecentFeatureRelease;
                        else if (release == AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb)
                            release = App.AvailableReleases.MostRecentLTSRelease;

                        Debug.WriteLine($"Checking for updates for {i.Path}");
                        string error_message = "";
                        var api_version = AdoptiumAPI.GetLatestVersion(release, i.JVM_Implementation, i.ImageType, i.HeapSize, out error_message, i.Arch, i.OS);
                        if (api_version.Found)
                        {
                            Debug.WriteLine($"API: {api_version.ReleaseName}");
                            if (api_version.ReleaseName != i.SkippedReleaseName || force)
                                if (api_version > i.InstalledVersion || force)
                                {
                                    Debug.WriteLine($"[API]{api_version.VersionString} > [Local]{i.InstalledVersion.VersionString}");

                                    i.NewVersion = api_version;
                                    i.MarkedForUpdate = api_version.ReleaseName != i.SkippedReleaseName; // don't mark if skipped

                                    // reset skipped release btw
                                    if (api_version.ReleaseName != i.SkippedReleaseName && api_version > i.InstalledVersion)
                                        i.SkippedReleaseName = null;

                                    there_are_new_versions = true;
                                }
                        }
                        else
                        {
                            errors_occured = true;
                            string name = (i.NotInstalled ?  $"New installation " : $"[{i.InstalledVersionString}]@[{i.Path}]")
                                        + $"[{ i.WatchedRelease}/{ i.ImageType}/{ i.JVM_Implementation}/{ i.HeapSize}/{ i.Arch}/{ i.OS}]";
                            string message = $"{name} => New Version not found: {error_message}";
                            ErrorsEncountered.Add(message);
                            Debug.WriteLine($"API: Not found: {message}");
                        }
                    }

                    watch.Stop();
                    Debug.WriteLine($"All {machine.Installations.Count} installations checked in {watch.ElapsedMilliseconds} ms.");
                }
                catch (Exception ex)
                {
                    errors_occured = true;
                    ErrorsEncountered.Add(ex.Message);                    

                    //if (force)
                    //    MessageBox.Show($"There was an error: [{ex.Message}].", Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                if (there_are_new_versions)
                    ThereAreNewVersions?.Invoke(this, EventArgs.Empty);
                else
                    NoNewVersions?.Invoke(this, EventArgs.Empty);

                if (errors_occured)
                    ErrorsOccuredWhileChecking?.Invoke(this, EventArgs.Empty);

                CheckComplete?.Invoke(this, EventArgs.Empty);
            };

            worker.RunWorkerAsync();

        }

        
            
    }
}
