using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    public enum InstallationUpdateStatus
    {
        UpdateComplete = -3,
        UpdateNotStarted = -2,
        UpdateIndeterminate = -1,
    }

    public class Installation : ViewModelBase
    {
        private bool detected;
        public bool initialized = false;

        public bool check_for_updates_flag;
        private string path = "";

        private AdoptiumReleaseVersion installed_version;

        private string watched_release;
        private string jvm_implementation;
        private string image_type;
        private string architecture;
        private string heap_size;
        private string os;

        private string suggested_version_string = null;
        private string suggested_jvm_implementation = null;
        private string suggested_image_type = null;
        private bool? suggested_x64 = null;



        private AdoptiumReleaseVersion new_available_version;
        private string skipped_release_name;

        public bool java_home_instance = false;
        public bool registry_autodiscovered_instance = false;
        private bool markedForUpdate;

        public bool Detected { get { return detected; } }
        public bool NotInstalled {  get { return String.IsNullOrEmpty(Path) && !Detected; } }
        public bool IsEditable { get { return !registry_autodiscovered_instance; } }
        public bool IsPathEditable { get { return !(java_home_instance || registry_autodiscovered_instance); } }

        public bool IsJavaHomeInstance { get { return java_home_instance; } }
        public bool IsRegistryAutodiscoveredInstance { get { return registry_autodiscovered_instance; } }
        public bool IsAutodiscoveredInstance { get { return registry_autodiscovered_instance; } }
        public bool RegistryUserScope { get; set; }




        public bool CheckForUpdatesFlag
        {
            get { return check_for_updates_flag; }
            set
            {
                check_for_updates_flag = value;
                OnPropertyChanged("CheckForUpdatesFlag");
                OnPropertyChanged("DisplayPath");
                OnPropertyChanged("InstallationTypeText");

                if (NotInstalled && check_for_updates_flag == false)
                {
                    MarkedForUpdate = false;
                    NewVersion = null;
                    //OnPropertyChanged("HasNewVersion");
                }
            }
        }
        public string InstallationTypeText
        {
            get
            {
                if (registry_autodiscovered_instance)
                    return "Registry " + (RegistryUserScope ? "(HKCU)" : "(HKLM)");
                else if (java_home_instance)
                    return "JAVA_HOME";
                else
                    return Detected || (NotInstalled && CheckForUpdatesFlag) ? "User added" : " ";
            }
        }





        public string DisplayPath
        {
            get { return NotInstalled ? (CheckForUpdatesFlag ? "New installation (pending)" : "Set the checkbox to install new release or select existing dir") : path; }
            set
            {
                if (!java_home_instance)
                {
                    Path = value;
                    OnPropertyChanged("DisplayPath");
                }
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                // when we change path, we also try to detect whether something is installed there
                detected = CheckPath(value);

                path = value;

                OnPropertyChanged("Path");
                OnPropertyChanged("DisplayPath");
                OnPropertyChanged("Detected");
                OnPropertyChanged("NotInstalled");

                // if object was not initialized before, we say it is initialised now
                if (!initialized)
                {
                    if (!NotInstalled)
                        initialized = true;

                    // we also set CheckForUpdatesFlag to detected, if object was not previously initialized
                    CheckForUpdatesFlag = detected;
                    // when we load data from XML initialized will be set to true, so we all these detections won't activate

                    // if installation is successfull detected at path and it is not autodiscovered -  && !IsAutodiscoveredInstance
                    // [!] this is not yet true cause we cannot get heap size from registry
                    if (detected)
                        SetReleaseParametersFromPath();
                }

                OnPropertyChanged("InstalledVersion");
                OnPropertyChanged("InstalledVersionMajor");
                OnPropertyChanged("InstalledVersionString");
                OnPropertyChanged("InstallationTypeText");
            }
        }
        public void SetReleaseParametersFromPath()
        {
            if (!detected) return;

            WatchedRelease = installed_version.Major.ToString();

            if (installed_version.Arch != "")
                Arch = installed_version.Arch;

            if (installed_version.JVMImplementation != "")
                JVM_Implementation = installed_version.JVMImplementation;

            if (installed_version.ImageType != "")
                ImageType = installed_version.ImageType;

            if (installed_version.Heap != "")
                HeapSize = installed_version.Heap;

        }
        public bool CheckPath(string path)
        {
            installed_version = new AdoptiumReleaseVersion();
            installed_version.LocalPath = path;

            if (!File.Exists(System.IO.Path.Combine(path, "release"))) return false;


            bool found = false;
            bool seems_to_be_release_file = true;
            bool seems_to_be_adoptopenjdk = false;

            ////// make sure its openjdk
            ////// currently we rely on 'adopt' keyword in path ...
            ////if (path.IndexOf("adopt", StringComparison.OrdinalIgnoreCase) >= 0 ||
            ////    path.IndexOf("openjdk", StringComparison.OrdinalIgnoreCase) >= 0)
            ////    seems_to_be_adoptopenjdk = true;

            ////// ... or 'ASSEMBLY_EXCEPTION' file with 'openjdk' keyword inside
            ////if (!seems_to_be_adoptopenjdk)
            ////{
            ////    string ASSEMBLY_EXCEPTION = Path.Combine(path, "ASSEMBLY_EXCEPTION");
            ////    if (File.Exists(ASSEMBLY_EXCEPTION))
            ////        if (File.ReadLines(ASSEMBLY_EXCEPTION).Any(line => line.IndexOf("openjdk", StringComparison.OrdinalIgnoreCase) >= 0 ))
            ////            seems_to_be_adoptopenjdk = true;
            ////}

            // we found the file - let's check its format
            if (seems_to_be_release_file)
            {
                Regex java_version_regex        = new Regex(@"JAVA_VERSION\s*=\s*""(.+)""",     RegexOptions.IgnoreCase);
                Regex semantic_version_regex    = new Regex(@"SEMANTIC_VERSION\s*=\s*""(.+)""", RegexOptions.IgnoreCase);
                Regex full_version_string_regex = new Regex(@"FULL_VERSION\s*=\s*""(.+)""",     RegexOptions.IgnoreCase);
                Regex image_type_regex          = new Regex(@"IMAGE_TYPE\s*=\s*""(.+)""",       RegexOptions.IgnoreCase);
                Regex jvm_impl_regex            = new Regex(@"JVM_VARIANT\s*=\s*""(.+)""",      RegexOptions.IgnoreCase);
                Regex heap_size_regex           = new Regex(@"HEAP_SIZE\s*=\s*""(.+)""",        RegexOptions.IgnoreCase);
                Regex os_arch_regex             = new Regex(@"OS_ARCH\s*=\s*""(.+)""",          RegexOptions.IgnoreCase);
                Regex os_name_regex             = new Regex(@"OS_NAME\s*=\s*""(.+)""",          RegexOptions.IgnoreCase);
                Regex source_regex              = new Regex(@"SOURCE\s*=\s*""(.+)""",           RegexOptions.IgnoreCase);

                bool semantic_parsed_successfully = false;
                bool full_version_processed = false;

                installed_version.Arch = "";
                installed_version.JVMImplementation = "";
                installed_version.ImageType = "";
                installed_version.Heap = "";

                StreamReader reader = File.OpenText(System.IO.Path.Combine(path, "release"));

                // initially, found = false
                // should change to true when at least something becomes available

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // JAVA_VERSION
                    // least informative source
                    // we need it only if there's nothing else available
                    Match match_java_version = java_version_regex.Match(line);
                    if (match_java_version.Success)
                    {
                        // least informative => use only in case nothing is available yet
                        if (!found)
                            found = installed_version.TrySetVersionFromString(match_java_version.Groups[1].Value);
                    }

                    // FULL_VERSION (string -> installed_version_string)
                    // more informative (provided it is available)
                    // so try anyway, unless semantic has been parsed already
                    Match match_full_version = full_version_string_regex.Match(line);
                    if (match_full_version.Success)
                    {
                        // update found, unless semantic has been parsed already
                        if (!semantic_parsed_successfully)
                        {
                            bool result = installed_version.TrySetVersionFromString(match_full_version.Groups[1].Value);
                            // if JAVA_VERSION did find something, and this did not, we should use at least some available data
                            found = found || result;
                        }

                        // anyway, set VersionString (highest priority)
                        installed_version.VersionString = match_full_version.Groups[1].Value;
                        full_version_processed = true;
                    }

                    // SEMANTIC_VERSION
                    // theoretically, the most informative source (provided it is available)
                    Match match_semantic_version = semantic_version_regex.Match(line);
                    if (match_semantic_version.Success)
                    {
                        bool result = installed_version.TrySetVersionFromString(match_semantic_version.Groups[1].Value);

                        // if SEMANTIC_VERSION is parsed correctly, prefer these data over all other
                        if (result)
                        {
                            semantic_parsed_successfully = true;
                            found = true;
                        }

                        // try to set VersionString (low priority)
                        if (!full_version_processed)
                            installed_version.VersionString = match_semantic_version.Groups[1].Value;
                    }


                    // IMAGE_TYPE
                    Match match_image_type = image_type_regex.Match(line);
                    if (match_image_type.Success)
                    {
                        installed_version.ImageType = match_image_type.Groups[1].Value.ToLowerInvariant();
                    }

                    // JVM_VARIANT
                    Match match_jvm_impl = jvm_impl_regex.Match(line);
                    if (match_jvm_impl.Success)
                    {
                        installed_version.JVMImplementation = match_jvm_impl.Groups[1].Value.ToLowerInvariant();
                    }

                    // HEAP_SIZE
                    Match match_heap_size = heap_size_regex.Match(line);
                    if (match_heap_size.Success)
                    {
                        installed_version.Heap = "normal";
                        if (match_heap_size.Groups[1].Value.ToLowerInvariant() == "large")
                            installed_version.Heap = "large";
                    }

                    // OS_ARCH
                    Match match_arch = os_arch_regex.Match(line);
                    if (match_arch.Success)
                    {
                        string os_arch_string = match_arch.Groups[1].Value;

                        if (os_arch_string.IndexOf("x86_64", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            os_arch_string.IndexOf("amd64", StringComparison.OrdinalIgnoreCase) >= 0)
                            installed_version.Arch = "x64";
                        else if (os_arch_string.IndexOf("i586", StringComparison.OrdinalIgnoreCase) >= 0)
                            installed_version.Arch = "x32";
                    }

                    // OS_NAME
                    Match match_os_name = os_name_regex.Match(line);
                    if (match_os_name.Success)
                    {
                        installed_version.OS = match_os_name.Groups[1].Value.ToLowerInvariant();
                    }

                    // SOURCE (failsafe for installed_version_jvm_implementation)
                    Match match_source = source_regex.Match(line);
                    if (match_source.Success && String.IsNullOrEmpty(installed_version.JVMImplementation))
                    {
                        string source_string = match_source.Groups[1].Value;

                        if (source_string.IndexOf("openj9", StringComparison.OrdinalIgnoreCase) >= 0)
                            installed_version.JVMImplementation = "openj9";
                        else if (source_string.IndexOf("hotspot", StringComparison.OrdinalIgnoreCase) >= 0)
                            installed_version.JVMImplementation = "hotspot";
                    }
                }

            } // if (seems_to_be_adoptopenjdk)           

            if (found)
            {
                // check for suggested version
                if (!String.IsNullOrEmpty(suggested_version_string))
                {
                    var version_elements = suggested_version_string.Split('.');
                    if (version_elements.Length == 4 &&
                        version_elements[0] == installed_version.Major.ToString() &&
                        version_elements[1] == installed_version.Minor.ToString() &&
                        version_elements[2] == installed_version.Security.ToString() &&
                        !String.IsNullOrEmpty(version_elements[3])
                        )
                    {
                        installed_version.MSIRevision = Convert.ToInt32(version_elements[3]);
                    }
                }

                // suggested Image Type should be prevalent
                if (!String.IsNullOrEmpty(suggested_image_type))
                    installed_version.ImageType = suggested_image_type.ToLowerInvariant();
                else
                {
                    // if we still do not know image type, let's try this:
                    if (String.IsNullOrEmpty(installed_version.ImageType))
                    {
                        installed_version.ImageType = "";
                        if (File.Exists(System.IO.Path.Combine(path, @"bin\javac.exe")))
                            installed_version.ImageType = "jdk";
                        else if (File.Exists(System.IO.Path.Combine(path, @"bin\java.exe")))
                            installed_version.ImageType = "jre";
                    }
                }

                // same for JVM Implementation
                if (!String.IsNullOrEmpty(suggested_jvm_implementation))
                    installed_version.JVMImplementation = suggested_jvm_implementation.ToLowerInvariant();
                else
                {
                    //one more check for robustness
                    if (installed_version.JVMImplementation == "")
                        installed_version.JVMImplementation = Directory.Exists(System.IO.Path.Combine(path, @"bin\j9vm")) ? "openj9" : "hotspot";
                }

                // same for arch:
                if (suggested_x64.HasValue && suggested_x64 != null)
                {
                    installed_version.Arch = suggested_x64 == true ? "x64" : "x32";
                }


                // if we still do not know heap size, let's try this:
                if (String.IsNullOrEmpty(installed_version.Heap))
                {
                    installed_version.Heap = "normal";
                    if (installed_version.JVMImplementation == "openj9" && Directory.Exists(System.IO.Path.Combine(path, @"bin\default"))
                        && installed_version.Arch == "x64") // because 'default' on openj9-x32 means normal heap, not large
                        installed_version.Heap = "large";
                }


            }

            installed_version.Found = found;
            return found;
        }

        //private bool TryParseVersionREGEX(bool found, bool full_version_processed, Match match_version)
        //{
        //    if (!full_version_processed)
        //        installed_version_string = match_version.Groups[1].Value;

        //    string _a = match_version.Groups[2].Value;
        //    int a = Convert.ToInt32(String.IsNullOrEmpty(_a) ? "0" : _a);

        //    if (a != 0)
        //    {
        //        string _b = match_version.Groups[4].Value;
        //        int b = Convert.ToInt32(String.IsNullOrEmpty(_b) ? "0" : _b);

        //        string _c = match_version.Groups[6].Value;
        //        int c = Convert.ToInt32(String.IsNullOrEmpty(_c) ? "0" : _c);

        //        string _d = match_version.Groups[8].Value;
        //        int d = Convert.ToInt32(String.IsNullOrEmpty(_d) ? "-1" : _d);


        //        if (a == 1)
        //        {
        //            installed_version_major = b;
        //            installed_version_minor = c;
        //            installed_version_security = d;
        //        }
        //        else
        //        {
        //            installed_version_major = a;
        //            installed_version_minor = b;
        //            installed_version_security = c;
        //        }

        //        if (match_version.Groups[10].Value != "")
        //            installed_version_build = Convert.ToInt32(match_version.Groups[10].Value);

        //        found = true;
        //    }

        //    return found;
        //}

        public string WatchedRelease
        {
            get { return watched_release; }
            set
            {
                watched_release = value;
                OnPropertyChanged("WatchedRelease");
            }
        }
        public string JVM_Implementation
        {
            get { return jvm_implementation; }
            set
            {
                jvm_implementation = value;
                if (jvm_implementation != "openj9" && HeapSize == "large")
                    HeapSize = "normal";

                OnPropertyChanged("JVM_Implementation");
            }
        }
        public string ImageType
        {
            get { return image_type; }
            set
            {
                image_type = value;
                OnPropertyChanged("ImageType");
            }
        }
        public string Arch
        {
            get { return architecture; }
            set
            {
                architecture = value;
                OnPropertyChanged("Arch");
            }
        }
        public string HeapSize
        {
            get { return heap_size; }
            set
            {
                heap_size = jvm_implementation == "openj9" ? value : "normal";
                OnPropertyChanged("HeapSize");
            }
        }
        public string OS
        {
            get { return os; }
        }



        public string SkippedReleaseName
        {
            get { return skipped_release_name; }
            set
            {
                skipped_release_name = value;
                OnPropertyChanged("SkippedReleaseName");
                OnPropertyChanged("HasSkippedRelease");
            }
        }
        public bool HasSkippedRelease { get { return !String.IsNullOrEmpty(SkippedReleaseName); } }

        internal void SkipDiscoveredNewVersion()
        {
            if (!HasNewVersion) return;

            SkippedReleaseName = NewVersion.ReleaseName;
            MarkedForUpdate = false;
            NewVersion = null;
            //OnPropertyChanged("HasNewVersion");
        }
        internal void RemoveSkippedRelease()
        {
            if (!HasSkippedRelease) return;
            SkippedReleaseName = null;
        }

        public AdoptiumReleaseVersion InstalledVersion
        {
            get
            {
                //AdoptiumReleaseVersion version = new AdoptiumReleaseVersion();

                //version.Major = installed_version_major.ToString();
                //version.Minor = installed_version_minor.ToString();
                //version.Security = installed_version_security.ToString();
                //version.Patch = (installed_version_patch == -1) ? null : installed_version_patch.ToString();
                //version.Build = (installed_version_build == -1) ? null : installed_version_build.ToString();

                //version.LocalPath = Path;
                //version.VersionString = installed_version_string;

                //version.Found = Detected;

                return installed_version;
            }
        }
        public string InstalledVersionMajor { get { return NotInstalled ? "+" : installed_version.Major.ToString(); } }
        public string InstalledVersionString
        {
            get
            {
                if (!detected)
                    return String.IsNullOrEmpty(path) ? "None yet" : "Not detected";
                else
                    return installed_version.ParsedVersionString;
            }
        }

        public AdoptiumReleaseVersion NewVersion
        {
            get { return new_available_version; }
            set
            {
                new_available_version = value;
                OnPropertyChanged("NewVersion");
                OnPropertyChanged("HasNewVersion");
                OnPropertyChanged("HasMSIInNewVersion");
                OnPropertyChanged("HasNewVersionButNoMSI");
            }
        }
        public bool HasNewVersion { get { return new_available_version?.Found ?? false; } }
        public bool HasMSIInNewVersion { get { return new_available_version != null ? !String.IsNullOrEmpty(new_available_version.MSIURL) : false; } }
        public bool HasNewVersionButNoMSI { get { return new_available_version != null && String.IsNullOrEmpty(new_available_version.MSIURL); } }
        public bool MarkedForUpdate 
        { 
            get => markedForUpdate;
            set
            {
                markedForUpdate = HasMSIInNewVersion ? value : false;
                OnPropertyChanged("MarkedForUpdate");
            }
        }

        public long UpdateBytesReceived { get; set; }
        public long UpdateTotalBytesToReceive { get; set; }
        private int _update_progress_percentage = (int)InstallationUpdateStatus.UpdateNotStarted;
        public int UpdateProgressPercentage
        {
            get { return _update_progress_percentage; }
            set
            {
                _update_progress_percentage = value;
                OnPropertyChanged("UpdateProgressPercentage");
                OnPropertyChanged("UpdateInProgress");
                OnPropertyChanged("UpdateIsIndeterminate");
                OnPropertyChanged("UpdateComplete");
            }
        }
        public bool UpdateInProgress { get { return _update_progress_percentage > (int)InstallationUpdateStatus.UpdateNotStarted; } }
        public bool UpdateIsIndeterminate { get { return _update_progress_percentage == (int)InstallationUpdateStatus.UpdateIndeterminate; } }
        public bool UpdateIsComplete { get { return _update_progress_percentage == (int)InstallationUpdateStatus.UpdateComplete; } }
        public void UpdateHasCompleted()
        {
            UpdateProgressPercentage = (int)InstallationUpdateStatus.UpdateComplete;
            NewVersion = null;
            MarkedForUpdate = false;
        }

        public Installation() : this(false) { } // empty constructor allows to add row

        public Installation(string _path) : this(_path, AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb, "hotspot", "jre", "x64", "normal", false) { }
        public Installation(bool _java_home_instance = false) : this("", AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb, "hotspot", "jre", "x64", "normal", _java_home_instance)
        { }

        public Installation(DiscoveredInstallation detectedInstallation, bool _registry_autodiscovered_instance, bool _registry_user_scope = false)
            : this(detectedInstallation.Path, AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb, "hotspot", "jre", "x64", "normal", false, _registry_autodiscovered_instance, _registry_user_scope, 
                  detectedInstallation.Version, detectedInstallation.JVM_Implementation, detectedInstallation.ImageType, detectedInstallation.x64) { }
        public Installation(string _path, bool _registry_autodiscovered_instance, bool _registry_user_scope = false, string _suggested_version_string = null)
            : this(_path, AdoptiumAPI_MostRecentVerbs.MostRecentLTSVerb, "hotspot", "jre", "x64", "normal", false, _registry_autodiscovered_instance, _registry_user_scope, _suggested_version_string) { }

        public Installation(
            string _path,
            string _watched_release,
            string _jvm_implementation,
            string _image_type,
            string _architecture,
            string _heap_size,
            bool _java_home_instance = false,
            bool _registry_autodiscovered_instance = false,
            bool _registry_user_scope = false,
            string _suggested_version_string = null,
            string _suggested_jvm_implementation = null,
            string _suggested_image_type = null,
            bool? _suggested_x64 = null
            )
        {
            os = "windows";
            MarkedForUpdate = false;
            UpdateProgressPercentage = -2;

            installed_version = new AdoptiumReleaseVersion();

            watched_release = _watched_release;
            jvm_implementation = _jvm_implementation;
            image_type = _image_type;
            architecture = _architecture;
            heap_size = _heap_size;

            java_home_instance = _java_home_instance;
            registry_autodiscovered_instance = _registry_autodiscovered_instance;
            RegistryUserScope = _registry_user_scope;

            suggested_version_string = _suggested_version_string;
            suggested_jvm_implementation = _suggested_jvm_implementation;
            suggested_image_type = _suggested_image_type;
            suggested_x64 = _suggested_x64;

            if (java_home_instance)
                CheckJavaHome();
            else
                Path = _path;
        }


        public bool CheckJavaHome()
        {
            bool result = false;
            var java_home = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (!String.IsNullOrEmpty(java_home))
            {
                Path = java_home;
                result = detected;
            }

            return result;
        }
        

    }
}
