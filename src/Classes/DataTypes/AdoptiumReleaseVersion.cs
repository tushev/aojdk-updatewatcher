using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    public class AdoptiumReleaseVersion
    {
        private int version_major       = 0;
        private int version_minor       = -1;
        private int version_security    = -1;
        private int version_patch       = -1;
        private int version_build       = -1;
        private int version_adopt_build = -1;

        private int version_msi_revision = -1; // currently not computed automatically

        private string version_string = "";
        private string version_full_version_string = "";
        private string version_semver = "";

        private bool found = false;

        public int Major {
            get { return version_major; } 
            set { version_major = value; }
        }
        public int Minor
        {
            get { return version_minor == -1 ? 0 : version_minor; }
            set { version_minor = value; }
        }
        public int Security
        {
            get { return version_security == -1 ? 0 : version_security; }
            set { version_security = value; }
        }
        public int Patch
        {
            get { return version_patch == -1 ? 0 : version_patch; }
            set { version_patch = value; }
        }
        public bool HasPatch { get { return version_patch > -1; } }
        public int Build
        {
            get { return version_build == -1 ? 0 : version_build; }
            set { version_build = value; }
        }
        public bool HasBuild { get { return version_build > -1; } }
        public int AdoptBuild
        {
            get { return version_adopt_build == -1 ? 0 : version_adopt_build; }
            set { version_adopt_build = value; }
        }
        public bool HasAdoptBuild { get { return version_adopt_build > -1; } }
        public int MSIRevision
        {
            get { return version_msi_revision == -1 ? 0 : version_msi_revision; }
            set { version_msi_revision = value; }
        }
        public bool HasMSIRevision {  get { return version_msi_revision > -1; } }

        public string VersionString { get; set; } // like: "openjdk_version": "1.8.0_275-b01" // "11.0.9+11" :: version only
        public string ReleaseName { get; set; } // like: "release_name": "jdk8u275-b01" // "jdk-11.0.8+10_openj9-0.21.0" // "jdk-11.0.9+11.1" :: extra info
        public string SemVerAPI { get; set; } // like: "semver": "8.0.275+1" // "11.0.9+11.1" :: extra info
        public string SemanticVersionRELEASE { get; set; } // like: SEMANTIC_VERSION="11.0.9.1+1" in RELEASE file

        public string MSIURL { get; set; }
        public string ZIPURL { get; set; }
        public string LocalPath { get; set; }

        public string ImageType { get; set; }
        public string Heap { get; set; }
        public string JVMImplementation { get; set; }
        public string Arch { get; set; }
        public string OS { get; set; }

        public string TypeOrEdition { get { return AdoptiumTools.GetTypeOrEditionFromAdoptiumReleaseVersion(this); } }
        public string TypeOrEditionText { get { return AdoptiumTools.GetTypeOrEditionString(TypeOrEdition); } }
        public string ImplementorRAW { get; set; }
        public string ImplementorVersionRAW { get; set; }

        public bool Found
        {
            get { return found; }
            set { found = value; }
        }

        public AdoptiumReleaseVersion() { }
        public AdoptiumReleaseVersion(int major, int minor, int security, int build = -1) : this(major, minor, security, -1, build) { }
        public AdoptiumReleaseVersion(int major, int minor, int security, int patch, int build, int adopt_build = -1) 
        {
            version_major = major;
            version_minor = minor;
            version_security = security;
            version_patch = patch;
            version_build = build;
            version_adopt_build = adopt_build;
        }

        public AdoptiumReleaseVersion(string _version_string) { TrySetVersionFromString(_version_string); }
        public bool TrySetVersionFromString(string _version_string) 
        {
            // AA.BB.CC.DD_EE-bFF+GG.HH
            Regex version_regex = new Regex(@"([0-9]+)(\.([0-9]+))?(\.([0-9]+))?(\.([0-9]+))?(_([0-9]+))?(\-b([0-9]+))?(\+([0-9]+)(\.([0-9]+))?)?", RegexOptions.IgnoreCase);
            const int m_AA              = 1;
            const int m_BB              = 3;
            const int m_CC              = 5;
            const int m_DD_patch        = 7;
            const int m_EE_sec_8        = 9;
            const int m_FF_build_8      = 11;
            const int m_GG_build        = 13;
            const int m_HH_adopt_build  = 15;

            bool parsed = false;

            // !! PREVIOUS VALUES OF NOT MATCHED ITEMS ARE NOT CHANGED
            // !! build overrides build_8

            Match match_version = version_regex.Match(_version_string);
            if (match_version.Success)
            {
                if (match_version.Groups[m_AA].Success)
                {
                    int a = Convert.ToInt32(match_version.Groups[m_AA].Value);

                    if (a == 1) // major <= 8
                    {
                        if (match_version.Groups[m_BB].Success)
                            version_major = Convert.ToInt32(match_version.Groups[m_BB].Value);

                        if (match_version.Groups[m_CC].Success)
                            version_minor = Convert.ToInt32(match_version.Groups[m_CC].Value);

                        if (match_version.Groups[m_EE_sec_8].Success)
                            version_security = Convert.ToInt32(match_version.Groups[m_EE_sec_8].Value);

                        if (match_version.Groups[m_FF_build_8].Success)
                            version_build = Convert.ToInt32(match_version.Groups[m_FF_build_8].Value);
                    }
                    else
                    {
                            version_major = a;

                        if (match_version.Groups[m_BB].Success)
                            version_minor = Convert.ToInt32(match_version.Groups[m_BB].Value);

                        if (match_version.Groups[m_CC].Success)
                            version_security = Convert.ToInt32(match_version.Groups[m_CC].Value);

                        if (match_version.Groups[m_DD_patch].Success)
                            version_patch = Convert.ToInt32(match_version.Groups[m_DD_patch].Value);
                    }

                    parsed = true;
                }

                if(match_version.Groups[m_GG_build].Success)
                    version_build = Convert.ToInt32(match_version.Groups[m_GG_build].Value);

                if(match_version.Groups[m_HH_adopt_build].Success)
                    version_adopt_build = Convert.ToInt32(match_version.Groups[m_HH_adopt_build].Value);
            }

            return parsed;
        }

        // return app-parsed string representation for version
        public string ParsedVersionString
        {
            get
            {
                return $"{Major}" +
                    (version_minor          == -1 ? "" : $".{version_minor}") + 
                    (version_security       == -1 ? "" : $".{version_security}") + 
                    (version_patch          == -1 ? "" : $".{version_patch}") + 
                    (version_build          == -1 ? (version_msi_revision == -1 ? "" : $"+[{version_msi_revision}]")
                                                       : $"+{version_build}") + 
                    (version_adopt_build    == -1 ? "" : $".{version_adopt_build}") 
                    ;
            }
        }

        public static bool operator > (AdoptiumReleaseVersion a, AdoptiumReleaseVersion b)
        {
            if (a.Major > b.Major) return true;
            if (a.Major < b.Major) return false;

            if (a.Minor > b.Minor) return true;
            if (a.Minor < b.Minor) return false;

            if (a.Security > b.Security) return true;
            if (a.Security < b.Security) return false;

            // by default, we compare for MSI revision
            if (a.HasMSIRevision && b.HasMSIRevision)
            {
                if (a.MSIRevision > b.MSIRevision) return true;
                // ! if result is < , we MUST continue comparing (otherwise we would get 8.0.308+8 == 8.0.308+8.1 )
            }

            if (a.Patch > b.Patch) return true;
            if (a.Patch < b.Patch) return false;

            if (a.Build > b.Build) return true;
            if (a.Build < b.Build) return false;

            if (a.AdoptBuild > b.AdoptBuild) return true;
            if (a.AdoptBuild < b.AdoptBuild) return false;

            return false;
        }
        public static bool operator < (AdoptiumReleaseVersion a, AdoptiumReleaseVersion b)
        {
            throw new NotImplementedException("TRYING TO COMPARE VERSIONS WITH < OPERATOR. RE-CONSIDER YOUR CODE OR CONTACT DEVELOPER.");

            /*
            if (Convert.ToInt32(a.Major) < Convert.ToInt32(b.Major)) return true;
            if (Convert.ToInt32(a.Major) > Convert.ToInt32(b.Major)) return false;

            if (Convert.ToInt32(a.Minor) < Convert.ToInt32(b.Minor)) return true;
            if (Convert.ToInt32(a.Minor) > Convert.ToInt32(b.Minor)) return false;

            if (Convert.ToInt32(a.Security) < Convert.ToInt32(b.Security)) return true;
            if (Convert.ToInt32(a.Security) > Convert.ToInt32(b.Security)) return false;


            if (!String.IsNullOrEmpty(a.Build) && !String.IsNullOrEmpty(b.Build) && Convert.ToInt32(a.Build) < Convert.ToInt32(b.Build)) return true;
            else return false;
            */
        }
    }
}
