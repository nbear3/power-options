using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace PowerOptions
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new TaskBarApp());
        }
    }

    public class TaskBarApp : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private Dictionary<MenuItem, System.Drawing.Icon> iconMap;

        public TaskBarApp()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Text = "Power Options",
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("High Performance", (s, e) => ChangePowerOption(s, e, "/s SCHEME_MIN")),
                new MenuItem("Balanced", (s, e) => ChangePowerOption(s, e, "/s SCHEME_BALANCED")),
                new MenuItem("Power Saver", (s, e) => ChangePowerOption(s, e, "/s SCHEME_MAX")),
            }),
                Visible = true
            };
            trayIcon.ContextMenu.MenuItems.Add("-");
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("E&xit", Exit));

            iconMap = new Dictionary<MenuItem, System.Drawing.Icon>();
            iconMap.Add(trayIcon.ContextMenu.MenuItems[0], Properties.Resources.battery_high);
            iconMap.Add(trayIcon.ContextMenu.MenuItems[1], Properties.Resources.battery_mid);
            iconMap.Add(trayIcon.ContextMenu.MenuItems[2], Properties.Resources.battery_low);

            string res = PowerProcess("/GETACTIVESCHEME").Split('(').Last();
            switch (res)
            {
                case "High performance)":
                    trayIcon.ContextMenu.MenuItems[0].Checked = true;
                    trayIcon.Icon = Properties.Resources.battery_high;
                    break;
                case "Balanced)":
                    trayIcon.ContextMenu.MenuItems[1].Checked = true;
                    trayIcon.Icon = Properties.Resources.battery_mid;
                    break;
                case "Power saver)":
                    trayIcon.ContextMenu.MenuItems[2].Checked = true;
                    trayIcon.Icon = Properties.Resources.battery_low;
                    break;
            }
        }

        private string PowerProcess(string Arguments)
        {
            Process proc = new Process();
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "powercfg";
            proc.StartInfo.Arguments = Arguments;
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }

        private void ChangePowerOption(object sender, EventArgs e, string Arguments)
        {
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.Checked) return;
            trayIcon.ContextMenu.MenuItems[0].Checked = false;
            trayIcon.ContextMenu.MenuItems[1].Checked = false;
            trayIcon.ContextMenu.MenuItems[2].Checked = false;
            trayIcon.Icon = iconMap[menuItem];
            menuItem.Checked = true;

            PowerProcess(Arguments);
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }

}
