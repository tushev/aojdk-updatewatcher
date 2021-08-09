using Fclp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    public class CommandLineOpts
    {
        public bool _Success { get; set; }
        public bool OpenConfigurationWindow { get; set; }
        public bool DeleteTask { get; set; }
        public bool AskDeleteTask { get; set; }
        public bool SilentlyDeleteTask { get; set; }
        public bool ForceSetTask { get; set; }
        public bool SetTaskAskIfNonConsistent { get; set; }
        public bool RunExplicitCheckForUpdates { get; set; }
        public bool SchedulerDoNotCheckConsistency { get; set; }

        internal void EnforceConsistency()
        {
            SchedulerCommandLineTasks.EnforceConsistency(this);
        }
    }

    static class CommandLine
    {
        public static CommandLineOpts Parse(string[] Args)
        {
            var p = new FluentCommandLineParser<CommandLineOpts> { IsCaseSensitive = false };

            Args = ConvertArgsToConvention(Args);

            p.Setup(a => a.OpenConfigurationWindow).As("config").SetDefault(false);
            p.Setup(a => a.DeleteTask).As("deletetask").SetDefault(false);
            p.Setup(a => a.AskDeleteTask).As("askdeletetask").SetDefault(false);
            p.Setup(a => a.SilentlyDeleteTask).As("silentlydeletetask").SetDefault(false);
            p.Setup(a => a.ForceSetTask).As("forcesettask").SetDefault(false);
            p.Setup(a => a.SetTaskAskIfNonConsistent).As("settask_askifnonconsistent").SetDefault(false);
            p.Setup(a => a.RunExplicitCheckForUpdates).As("explicitcheck").SetDefault(false);
            p.Setup(a => a.SchedulerDoNotCheckConsistency).As("scheduler_donotcheckconsistency").SetDefault(false);

            var res = p.Parse(Args);

            var opts = p.Object;
            opts._Success = !res.HasErrors;

            opts.EnforceConsistency();

            return opts;
        }

        private static string[] ConvertArgsToConvention(string[] Args)
        {
            string[] old_params = {
                "-config",
                "-deletetask",
                "-askdeletetask",
                "-silentlydeletetask",
                "-forcesettask",
                "-settask_askifnonconsistent",
                "-explicitcheck", 
            };

            var converted_args = Args.Select(arg => 
                                        old_params.Contains(arg) ? 
                                                                $"-{arg}" : arg 
                                 ).ToArray();

            return converted_args;
        }
    }
}
