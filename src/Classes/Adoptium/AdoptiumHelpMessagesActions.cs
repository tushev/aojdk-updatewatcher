using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    static class AdoptiumHelpMessagesActions
    {
        static public void ShowJVM_ImplementationHelp()
        {
            MessageBox.Show(
                "HotSpot is the VM from the OpenJDK community." + Environment.NewLine +
                "It is the most widely used VM today and is used in Oracle’s JDK. It is suitable for all workloads." + Environment.NewLine + Environment.NewLine +
                "Eclipse OpenJ9 is the VM from the Eclipse community." + Environment.NewLine +
                "It is an enterprise-grade VM designed for low memory footprint and fast start-up and is used in IBM’s JDK. It is suitable for running all workloads.", "An advice from AdoptOpenJDK.net", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        static public void ShowHeapHelp()
        {
            MessageBox.Show(
              @"What are the OpenJ9 ""Large Heap"" variants ?" + Environment.NewLine + Environment.NewLine +
              @"The Large Heap variants of the OpenJ9 builds" + Environment.NewLine +
              @"(also known as the ""non-compressed references builds"")" + Environment.NewLine +
              @"allow for Java heap sizes greater than 57Gb." + Environment.NewLine + Environment.NewLine +
              @"If you need heap sizes that large, then pick the large heap versions.", "An advice from AdoptOpenJDK.net/faq.html", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        static public void ShowWhatJREJDKHelp()
        {
            Process.Start("https://stackoverflow.com/questions/1906445/what-is-the-difference-between-jdk-and-jre/#1906455");
        }

        static public void ShowLTSHelp()
        {
            Process.Start("https://adoptopenjdk.net/support.html");
        }

        static public void OpenMainWebPage()
        {
            Process.Start("https://adoptopenjdk.net/");
        }
        static public void OpenMovingWebPage()
        {
            Process.Start("https://blog.adoptium.net/2021/04/Adoptium-to-promote-broad-range-of-compatible-OpenJDK-builds/");
        }
    }
}
