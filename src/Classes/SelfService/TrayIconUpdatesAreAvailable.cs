using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AJ_UpdateWatcher
{
    static class TrayIconUpdatesAreAvailable
    {
        static private System.Windows.Forms.NotifyIcon trayIcon;

        static public event EventHandler UserClickedOnIconOrNotification;

        static public void ShowNotification(string text = "Click for more info", int delay_ms = 10000, int second_reminder_ms = 60000)
        {
            if (trayIcon == null)
            {
                trayIcon = new System.Windows.Forms.NotifyIcon();

                trayIcon.Click += (s, e) => { IconClicked(e); };
                trayIcon.MouseUp += (s, e) => { IconClicked(e); };
                trayIcon.BalloonTipClicked += (s, e) => { IconClicked(e); };
            }

            trayIcon.Text = Branding.ProductName;
            trayIcon.BalloonTipTitle = $"New {Branding.TargetProduct} versions available";

            trayIcon.Visible = true;

            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Warning;

            trayIcon.BalloonTipText = text;
            trayIcon.ShowBalloonTip(delay_ms);
        }

        static void IconClicked(EventArgs e)
        {
            UserClickedOnIconOrNotification?.Invoke(null, e);
            RemoveTrayIcon();

            if (UserClickedOnIconOrNotification == null)
                System.Windows.Application.Current.Shutdown();
        }

        static public void RemoveTrayIcon() 
        {
            if (trayIcon != null)
                trayIcon.Dispose();

            trayIcon = null;
        }
    }
}
