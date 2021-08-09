using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.Windows;

namespace AJ_UpdateWatcher
{
    public class SchedulerManager : ViewModelBase
    {
        // https://github.com/dahall/TaskScheduler/wiki/Examples

        const string defaultTaskName = "Adopt_UpdateWatcher_TskV2";

        string app_path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        public bool TaskStatus
        {
            get { return TaskIsSet(); }
            set
            {
                if (value && !TaskIsSet())
                    InstallTask();

                if (!value && TaskIsSet())
                    DeleteTask();

                OnPropertyChanged("TaskStatus");
            }
        }

        public bool TaskIsSet(string taskName = defaultTaskName)
        {
            Microsoft.Win32.TaskScheduler.Task t = TaskService.Instance.GetTask(taskName);
            return t != null;
        }

        public void CheckConsistency(string taskName = defaultTaskName)
        {
            Microsoft.Win32.TaskScheduler.Task t = TaskService.Instance.GetTask(taskName);
            if (t != null)
            {
                if (!(t.Definition.Actions.Count == 1 &&  t.Definition.Actions[0].ToString().Trim() == app_path))
                {
                    Debug.WriteLine($"ACT = [{t.Definition.Actions[0].ToString().Trim()}], comparison = [{t.Definition.Actions[0].ToString().Trim() == app_path}]");
                    if (! App.CommandLineOptions.SchedulerDoNotCheckConsistency )
                    {
                        var ans = MessageBox.Show(
                            $"There is a Task Scheduler task for {Branding.ProductName}, but it is not set exactly to run this very instance of updater." + Environment.NewLine + Environment.NewLine +
                            $"The task has {t.Definition.Actions.Count} action(s) and the first action is set to [{t.Definition.Actions[0].ToString().Trim()}]" + Environment.NewLine + Environment.NewLine +
                            $"This app is located at [{app_path}]" + Environment.NewLine + Environment.NewLine +
                            $"Click Yes to delete the task and re-create it correctly," + Environment.NewLine + "No to keep things as is.",
                            Branding.MessageBoxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (ans == MessageBoxResult.Yes)
                        {
                            ForceReInstallTask(taskName);
                        }
                    }
                }
            }
        }

        public void ForceReInstallTask(string taskName = defaultTaskName)
        {
            if (TaskIsSet())
                DeleteTask(taskName);

            InstallTask(taskName);
        }

        public void InstallTask(string taskName = defaultTaskName)
        {

            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = $"Checks for updates of {Branding.TargetProduct}";
            td.Principal.LogonType = TaskLogonType.InteractiveToken;

            // V2 only: Add a delayed logon trigger for a specific user
            LogonTrigger lt2 = td.Triggers.Add(new LogonTrigger { UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name });
            lt2.Delay = TimeSpan.FromMinutes(1);

            // Add an action that will launch Notepad whenever the trigger fires
            td.Actions.Add(app_path);

            // Register the task in the root folder            
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, td);
        }

        public void DeleteTask(string taskName = defaultTaskName)
        {
            // Remove the task we just created
            TaskService.Instance.RootFolder.DeleteTask(taskName);
        }

        public void EditTask(string taskName = defaultTaskName)
        {
            Microsoft.Win32.TaskScheduler.Task t = TaskService.Instance.GetTask(taskName);
            if (t == null) return;

            TaskEditDialog editorForm = new TaskEditDialog(t, true, true);
            editorForm.ShowDialog();
        }
    }
}
