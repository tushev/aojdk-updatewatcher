using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AJ_UpdateWatcher
{
    static class SchedulerCommandLineTasks
    {
        public static bool HasSingleTask
        {
            get
            {
                return HasDeleteTaskOption || HasSetTaskOption;
            }
        }

        private static bool HasDeleteTaskOption { get { return CheckHasDeleteTaskOption(App.CommandLineOptions); } }
        private static bool CheckHasDeleteTaskOption(CommandLineOpts o)
        {
                return  o.AskDeleteTask
                     || o.DeleteTask
                     || o.SilentlyDeleteTask;
        }
        private static bool HasSetTaskOption { get { return CheckHasSetTaskOption(App.CommandLineOptions); } }
        private static bool CheckHasSetTaskOption(CommandLineOpts o)
        {
                return o.ForceSetTask
                    || o.SetTaskAskIfNonConsistent;
        }

        internal static void EnforceConsistency(CommandLineOpts o)
        {
            if (CheckHasDeleteTaskOption(o))
            {
                o.ForceSetTask = false;
                o.SetTaskAskIfNonConsistent = false;
            }

            if (o.AskDeleteTask)
            {
                o.DeleteTask = false;
                o.SilentlyDeleteTask = false;
            } 
            else
            {
                if (o.DeleteTask)
                {
                    o.AskDeleteTask = false;
                    o.SilentlyDeleteTask = false;
                }
                else
                {
                    if (o.SilentlyDeleteTask)
                    {
                        o.AskDeleteTask = false;
                        o.DeleteTask = false;
                    }
                }
            }

            if (o.ForceSetTask)
            {
                o.SetTaskAskIfNonConsistent = false;
            }
            else
            {
                if (o.SetTaskAskIfNonConsistent)
                {
                    o.ForceSetTask = false;
                }
            }
        }

        static public void ProcessSingleTask()
        {
            if (HasDeleteTaskOption)
            {
                SchedulerManager sm = new SchedulerManager();
                if (sm.TaskIsSet())
                {
                    var result = App.CommandLineOptions.AskDeleteTask ?
                        MessageBox.Show($"You have a scheduled task to check for updates of {Branding.TargetProduct}.{Environment.NewLine + Environment.NewLine}Do you want to remove this scheduled task?", Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) :
                        MessageBoxResult.Yes;

                    if (result == MessageBoxResult.Yes)
                    {
                        sm.DeleteTask();

                        if (!App.CommandLineOptions.SilentlyDeleteTask)
                        {
                            MessageBox.Show("Scheduled task has been removed successfully", Branding.MessageBoxHeader, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }

            if (HasSetTaskOption)
            {
                SchedulerManager sm = new SchedulerManager();

                // re-create the task without asking
                if (App.CommandLineOptions.ForceSetTask)
                    sm.ForceReInstallTask();

                // ask if task is present and not-consistent, otherwise create it silently
                if (App.CommandLineOptions.SetTaskAskIfNonConsistent)
                {
                    if (sm.TaskIsSet())
                        sm.CheckConsistency();
                    else
                        sm.ForceReInstallTask();
                }
            }

        }
    }
}
