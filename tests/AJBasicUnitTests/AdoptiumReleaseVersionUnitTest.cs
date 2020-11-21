using AJ_UpdateWatcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AJBasicUnitTests
{
    [TestClass]
    public class AdoptiumReleaseVersionUnitTest
    {
        [TestMethod]
        public void MSIRevisionIsHandledCorrect()
        {
            AdoptiumReleaseVersion v;

            v = new AdoptiumReleaseVersion("11.0.9.1+11.2");
            Assert.AreEqual(false, v.HasMSIRevision);

            v = new AdoptiumReleaseVersion("1.8.0_275-b01");
            Assert.AreEqual(false, v.HasMSIRevision);

            v = new AdoptiumReleaseVersion("1.8.0_275-b01");
            v.MSIRevision = 1;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(1, v.MSIRevision);

            v = new AdoptiumReleaseVersion("11.0.7");
            v.MSIRevision = 11;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(11, v.MSIRevision);

            v = new AdoptiumReleaseVersion("11.0.9.1+1");
            v.MSIRevision = 101;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(101, v.MSIRevision);

            v = new AdoptiumReleaseVersion("11.0.9.1+11.2");
            v.MSIRevision = 11;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(11, v.MSIRevision);

            v = new AdoptiumReleaseVersion("15+36");
            v.MSIRevision = 11;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(11, v.MSIRevision);

            v = new AdoptiumReleaseVersion(11, 0, 9, 1, 11, 2);
            Assert.AreEqual(false, v.HasMSIRevision);

            v = new AdoptiumReleaseVersion(11, 0, 9, 1, 11, 2);
            v.MSIRevision = 7;
            Assert.AreEqual(true, v.HasMSIRevision);
            Assert.AreEqual(7, v.MSIRevision);
        }

        [TestMethod]
        public void ParsedVersionStringGenerationIsCorrect()
        {
            AdoptiumReleaseVersion v;

            /*
                FULL_VERSION="1.8.0_275-b01"
                JAVA_VERSION="11.0.9.1"
                FULL_VERSION="11.0.9.1+1"
                SEMANTIC_VERSION="11.0.9.1+1"
                JVM_VERSION="11.0.9.1+11.2"
                JVM_VERSION="11"
                VERSION="15+36"
            */

            v = new AdoptiumReleaseVersion("1.8.0_275");
            Assert.AreEqual("8.0.275", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("1.8.0_275-b01");
            Assert.AreEqual("8.0.275+1", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("1.8.0_275-b01");
            v.MSIRevision = 1;
            Assert.AreEqual("8.0.275+1", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.7");
            v.MSIRevision = 11;
            Assert.AreEqual("11.0.7+[11]", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.9.1");
            Assert.AreEqual("11.0.9.1", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.9.1+1");
            Assert.AreEqual("11.0.9.1+1", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.9.1+1");
            v.MSIRevision = 101;
            Assert.AreEqual("11.0.9.1+1", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.9.1+11.2");
            Assert.AreEqual("11.0.9.1+11.2", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11.0.9.1+11.2");
            v.MSIRevision = 11;
            Assert.AreEqual("11.0.9.1+11.2", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("11");
            Assert.AreEqual("11", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("15+36");
            Assert.AreEqual("15+36", v.ParsedVersionString);

            v = new AdoptiumReleaseVersion("15+36");
            v.MSIRevision = 11;
            Assert.AreEqual("15+36", v.ParsedVersionString);
        }

        [TestMethod]
        public void ConstructionIsCorrect()
        {
            AdoptiumReleaseVersion v = new AdoptiumReleaseVersion();
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 17, 22);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(17, v.Minor);
            Assert.AreEqual(22, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 17, 22, -1);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(17, v.Minor);
            Assert.AreEqual(22, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 17, 22, 5);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(17, v.Minor);
            Assert.AreEqual(22, v.Security);
            Assert.AreEqual(5, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 0, 9, 1, 11);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(11, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 0, 9, 1, 11, -1);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(11, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion(11, 0, 9, 1, 11, 2);
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(11, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(2, v.AdoptBuild);
            Assert.AreEqual(true, v.HasAdoptBuild);
        }

        [TestMethod]
        public void ParsingIsCorrect()
        {
            AdoptiumReleaseVersion v;

            /*
                FULL_VERSION="1.8.0_275-b01"
                JAVA_VERSION="11.0.9.1"
                FULL_VERSION="11.0.9.1+1"
                SEMANTIC_VERSION="11.0.9.1+1"
                JVM_VERSION="11.0.9.1+11.2"
                JVM_VERSION="11"
                VERSION="15+36"
            */

            v = new AdoptiumReleaseVersion("1.8.0_275");
            Assert.AreEqual(8, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(275, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("1.8.0_275-b01");
            Assert.AreEqual(8, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(275, v.Security);
            Assert.AreEqual(1, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("11.0.7");
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(7, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("11.0.9.1");
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("11.0.9.1+1");
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(1, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("11.0.9.1+11.2");
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(9, v.Security);
            Assert.AreEqual(11, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(1, v.Patch);
            Assert.AreEqual(true, v.HasPatch);
            Assert.AreEqual(2, v.AdoptBuild);
            Assert.AreEqual(true, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("11");
            Assert.AreEqual(11, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Security);
            Assert.AreEqual(0, v.Build);
            Assert.AreEqual(false, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

            v = new AdoptiumReleaseVersion("15+36");
            Assert.AreEqual(15, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Security);
            Assert.AreEqual(36, v.Build);
            Assert.AreEqual(true, v.HasBuild);
            Assert.AreEqual(0, v.Patch);
            Assert.AreEqual(false, v.HasPatch);
            Assert.AreEqual(0, v.AdoptBuild);
            Assert.AreEqual(false, v.HasAdoptBuild);

        }
    }
}
