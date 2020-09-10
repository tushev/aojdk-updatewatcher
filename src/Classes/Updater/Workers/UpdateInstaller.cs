using Adoptium_UpdateWatcher.Properties;
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
using System.Windows.Threading;

namespace Adoptium_UpdateWatcher
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
                    var path = data.Item1;

                    var p = Process.Start(path);
                    p.WaitForExit();

                    try { File.Delete(path); }
                    catch (Exception _e) { Debug.WriteLine($"Cannot delete [{path}]: {_e.Message}"); }

                    data.Item2.UpdateHasCompleted();

                    worker.ReportProgress(-1);                    
                });

                int ItemsToUpdateCount = machine.Installations.Where(i => i.HasNewVersion && i.MarkedForUpdate).Count();
                int ItemsDownloaded = 0;


                var watch = System.Diagnostics.Stopwatch.StartNew();

                Parallel.ForEach(machine.Installations, new ParallelOptions { 
                    MaxDegreeOfParallelism = Settings.Default.UserConfigurableSetting_MaxConcurrentMSIDownloads
                }, 
                    i =>
                {
                    try
                    {
                        if (i.HasNewVersion && i.MarkedForUpdate)
                        {
                            Debug.WriteLine($"Updating {i.Path} to {i.NewVersion.VersionString}");

                            Uri ur = new Uri(i.NewVersion.MSIURL);
                            string tempname = System.IO.Path.GetTempPath() + 
                                              (Settings.Default.UserConfigurableSetting_UseRandomPrefixForDownloadedMSIs ? Guid.NewGuid().ToString().Substring(0, 14) : "") +
                                              GetFileNameFromUrl(i.NewVersion.MSIURL);

                            i.UpdateProgressPercentage = (int)InstallationUpdateStatus.UpdateIndeterminate;

                            using (WebClient client = new WebClient())
                            {
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
