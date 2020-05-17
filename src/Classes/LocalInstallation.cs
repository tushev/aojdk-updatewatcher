using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using AdoptOpenJDK_UpdateWatcher.Properties;

namespace AdoptOpenJDK_UpdateWatcher
{
    static class LocalInstallation
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        static private string version_string = "";
        static public string VersionString
        {
            get { return version_string; }
            set
            {
                version_string = value;
                OnStaticPropertyChanged("VersionString");
            }
        }
        static private string installation_path = "";
        static public string InstallationPath
        {
            get { return installation_path; }
            set
            {
                installation_path = value;
                OnStaticPropertyChanged("InstallationPath");
            }
        }

        static public int       VersionMajor = 0;
        static public int       VersionMinor = 0;
        static public int       VersionSecurity = 0;
        static public bool      Detected = false;

        static public bool TryDetect()
        {
            bool detected = false;

            bool useJavaHome = Settings.Default.useJavaHome;

            if (useJavaHome)
                detected = CheckJavaHome();
            else
            {
                string path = Settings.Default.LocalInstallationPath;
                if (Directory.Exists(path))
                    detected = CheckPath(path);
            }

            return detected;
        }

        static public bool CheckJavaHome()
        {
            var java_home = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!String.IsNullOrEmpty(java_home))
                return CheckPath(java_home);
            else return false;
        }

        static public LatestVersion GetVersion()
        {
            LatestVersion version = new LatestVersion();

            version.Major = VersionMajor.ToString();
            version.Minor = VersionMinor.ToString();
            version.Security = VersionSecurity.ToString();
            version.Found = true;
            version.LocalPath = InstallationPath;
            version.VersionString = VersionString;

            return version;
        }

        static public bool CheckPath(string path, bool UpdateLocalInstallationInstance = true)
        {
            if (!File.Exists(Path.Combine(path, "release"))) return false;

            bool found = false;
            bool seems_to_be_adoptopenjdk = true;

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

            // we found the file
            if (seems_to_be_adoptopenjdk)
            {
                const string regex = "JAVA_VERSION=\\\"(([0-9]+)\\.([0-9]+)\\.([0-9]+)(_([0-9]+))?.*)\\\"";
                Regex java_version_regex = new Regex(regex, RegexOptions.IgnoreCase);

                StreamReader reader = File.OpenText(Path.Combine(path, "release"));

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = java_version_regex.Match(line);
                    if (match.Success)
                    {
                        if (UpdateLocalInstallationInstance)
                        {
                            VersionString = match.Groups[1].Value;

                            int a = Convert.ToInt32(match.Groups[2].Value);
                            int b = Convert.ToInt32(match.Groups[3].Value);
                            int c = Convert.ToInt32(match.Groups[4].Value);

                            int d = -1;
                            if (match.Groups[6].Value != "")
                                d = Convert.ToInt32(match.Groups[6].Value);


                            if (a == 1)
                            {
                                VersionMajor = b;
                                VersionMinor = c;
                                VersionSecurity = d;
                            }
                            else
                            {
                                VersionMajor = a;
                                VersionMinor = b;
                                VersionSecurity = c;
                            }
                        }
                        found = true;
                        break;
                    }
                }

            } // if (seems_to_be_adoptopenjdk)

            if (found && UpdateLocalInstallationInstance)
            {
                InstallationPath = path;
                Detected = true;
            }

            return found;
        }

    }
}
