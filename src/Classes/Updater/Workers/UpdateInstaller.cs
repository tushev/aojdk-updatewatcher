using AJ_UpdateWatcher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Threading;

namespace AJ_UpdateWatcher
{
    public class AdoptiumUpdateInstallerEventArgs : EventArgs
    {
        int percent = 0;
        bool percent_changed = false;
        public AdoptiumUpdateInstallerEventArgs()
        {

        }
        public AdoptiumUpdateInstallerEventArgs(int _percent)
        {
            percent = _percent;
            percent_changed = true;
        }
        
        public bool PercentChanged { get { return percent_changed; } }
        public int Percent { get { return percent; } }
    }
    public class UpdateInstaller
    {
        private bool errors_occured;

        public EventHandler ProgressChanged;
        public EventHandler UpdateProcessStarted;
        public EventHandler UpdateProcessCompleted;

        public List<string> ErrorsEncountered;
        public EventHandler ErrorsOccuredWhileUpdating;

        public UpdateInstaller()
        {
            ResetFlags();
        }

        private void ResetFlags()
        {
            ErrorsEncountered = new List<string>();
            errors_occured = false;
        }

        public void DownloadAndInstallUpdatesAsync(Machine machine)
        {
            UpdateProcessStarted?.Invoke(this, EventArgs.Empty);

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.ProgressChanged += (s, e) =>
            {
                AdoptiumUpdateInstallerEventArgs ea = e.ProgressPercentage == -1 ? new AdoptiumUpdateInstallerEventArgs() : new AdoptiumUpdateInstallerEventArgs(e.ProgressPercentage);
                ProgressChanged?.Invoke(this, ea);
            };

            worker.DoWork += (s, e) =>
            {
                var ExecuteAndWaitForExit = new ActionBlock<Tuple<string, Installation>>(data =>
                {
                    var msi_path = data.Item1;
                    var i = data.Item2;

                    var p = Process.Start(msi_path);
                    p.WaitForExit();

                    Debug.WriteLine($"Exit({p.ExitCode}) : {msi_path}");

                    // ERROR_SUCCESS	                0	    The action completed successfully.
                    // ERROR_SUCCESS_REBOOT_INITIATED	1641	The installer has initiated a restart. This message is indicative of a success.
                    // ERROR_SUCCESS_REBOOT_REQUIRED	3010	A restart is required to complete the install. This message is indicative of a success. This does not include installs where the ForceReboot action is run.
                    bool success = p.ExitCode == 0 || p.ExitCode == 1641 || p.ExitCode == 3010;

#if DEBUG
                    Debug.WriteLine($"[DEBUG] Overriding success({success}) = true");
                    success = true;
#endif

                    try { File.Delete(msi_path); }
                    catch (Exception _e) { Debug.WriteLine($"Cannot delete [{msi_path}]: {_e.Message}"); }

                    var dis = AdoptiumTransitionRouter.SuggestDisabling(data.Item2, data.Item2.NewVersion);
                    if (success && dis.Item1)
                    {
                        var ans = MessageBox.Show(
                            $"Update of " + Environment.NewLine + $"{i.DisplayPath}" + Environment.NewLine + 
                            $"from [{i.InstalledVersion.VersionString}] to [{i.NewVersion.VersionString}] has completed successfully." + Environment.NewLine + Environment.NewLine +
                            $"However, it is *possible* that the old version was not uninstalled correctly due to the following reason(s):" + Environment.NewLine + Environment.NewLine +
                            dis.Item2 + Environment.NewLine +
                            $"It is suggested to disable checking for updates for the old version to avoid duplicate update suggestion. You may also have to uninstall the old version manually." + Environment.NewLine + Environment.NewLine +
                            $"Would you like to disable checking for updates for {i.DisplayPath}?", 
                            Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (ans == MessageBoxResult.Yes)
                            App.Machine.InstallationPathsToDisable.Add(i.Path);
                    }

                    i.UpdateHasCompleted(success);

                    worker.ReportProgress(-1);                    
                });

                int ItemsToUpdateCount = machine.Installations.Where(i => i.HasNewVersion && i.MarkedForUpdate && !i.UpdateInProgress).Count();
                int ItemsDownloaded = 0;


                var watch = System.Diagnostics.Stopwatch.StartNew();

                Parallel.ForEach(machine.Installations, new ParallelOptions { 
                    MaxDegreeOfParallelism = Settings.Default.UserConfigurableSetting_MaxConcurrentMSIDownloads
                }, 
                    i =>
                {
                    try
                    {
                        if (i.HasNewVersion && i.MarkedForUpdate && !i.UpdateInProgress)
                        {
                            Debug.WriteLine($"Updating {i.Path} to {i.NewVersion.VersionString}");

                            Uri ur = new Uri(i.NewVersion.MSIURL);
                            string tempname = System.IO.Path.GetTempPath() + 
                                              (Settings.Default.UserConfigurableSetting_UseRandomPrefixForDownloadedMSIs ? Guid.NewGuid().ToString().Substring(0, 14) : "") +
                                              GetFileNameFromUrl(i.NewVersion.MSIURL);

                            i.UpdateProgressPercentage = (int)InstallationUpdateStatus.UpdateIndeterminate;

                            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                            defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

                            using (WebClient client = new WebClient { Proxy = defaultWebProxy })
                            {
                                //client.Proxy = ProxyConfigurator.GetWebProxy;

                                client.DownloadProgressChanged += (o, ep) =>
                                    {
                                        i.UpdateBytesReceived = ep.BytesReceived;
                                        i.UpdateTotalBytesToReceive = ep.TotalBytesToReceive;
                                        i.UpdateProgressPercentage = ep.ProgressPercentage;

                                        //worker.ReportProgress(-1);
                                    };
                                client.DownloadFileCompleted += (o, edp) =>
                                    {
                                        i.UpdateProgressPercentage = (int)InstallationUpdateStatus.UpdateIndeterminate;
                                        worker.ReportProgress(-1);

                                        ExecuteAndWaitForExit.Post(new Tuple<string, Installation>(tempname, i));

                                        var newValue = Interlocked.Increment(ref ItemsDownloaded);
                                        if (newValue == ItemsToUpdateCount)
                                            ExecuteAndWaitForExit.Complete();
                                    };

                                client.DownloadFileAsync(ur, tempname);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (ErrorsEncountered)
                        {
                            errors_occured = true;
                            ErrorsEncountered.Add(ex.Message);
                        }
                    }
                });

                ExecuteAndWaitForExit.Completion.Wait();

                watch.Stop();
                Debug.WriteLine($"All {ItemsToUpdateCount} installations updated in {watch.ElapsedMilliseconds} ms.");

            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                if (errors_occured)
                    ErrorsOccuredWhileUpdating?.Invoke(this, EventArgs.Empty);

                UpdateProcessCompleted?.Invoke(this, EventArgs.Empty);
            };

            worker.RunWorkerAsync();




        }
        static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            Uri SomeBaseUri = new Uri("http://canbeanything");

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return System.IO.Path.GetFileName(uri.LocalPath);
        }
    }
}
