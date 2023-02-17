using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Reflection;
using System.Threading;
using static TimeSheeter.Program;
using Newtonsoft.Json.Bson;
using System.Timers;
using System.IO;

namespace TimeSheeter
{

    static class GlobalParm
    {
        public static bool timerToggle = true;
        public static bool _ShowConsole = false;
        public static NotifyIcon notifyIcon = new NotifyIcon();
    }

    // ================================


    public class Program
    {


        private static DateTime startTime;
        private static DateTime endTime;


        [STAThread]
        internal static void Main()
        {
            StartTimeSheet();

            GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
            ExternalDll.SendMessage(Process.GetCurrentProcess().MainWindowHandle, ExternalDll.WM_SYSCOMMAND, ExternalDll.SC_MINIMIZE, 0);

            ExternalDll.SetConsoleWindowVisibility(GlobalParm._ShowConsole);

            GlobalParm.notifyIcon.Visible = true;
            GlobalParm.notifyIcon.Text = System.Windows.Forms.Application.ProductName;

            var contextMenu = new ContextMenuStrip();

            ContextMenu.SetContextMenu(contextMenu);

            GlobalParm.notifyIcon.ContextMenuStrip = contextMenu;

            System.Windows.Forms.Application.Run();
            GlobalParm.notifyIcon.Visible = false;
            
            ExternalDll.FreeConsole();

            if (GlobalParm.timerToggle)
            {
                EndTimeSheet();
            }

        }

        public static void ToggleTimeSheet()
        {
            if (GlobalParm.timerToggle)
            {
                EndTimeSheet();
                GlobalParm.timerToggle = false;
                GlobalParm.notifyIcon.Icon = new Icon("icon_red.ico");
            }
            else
            {
                StartTimeSheet();
                GlobalParm.timerToggle = true;
                GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
            }

        }


        private static void StartTimeSheet()
        {
            // Start tracking time
            startTime = DateTime.Now;
            endTime = DateTime.MinValue;
        }

        private static void EndTimeSheet()
        {
            // Stop tracking time
            endTime = DateTime.Now;

            // Save time value to file
            string timeValue = (endTime - startTime).ToString();
            string filePath = @"C:\LOCAL\TimeSheeter\time_detail.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(endTime.ToString() + " - " + startTime.ToString() + " = " + timeValue.ToString());
            }
            
            filePath = @"C:\LOCAL\TimeSheeter\time.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(startTime.ToString() + " to " + endTime.ToString());
            }

        }

 
    
    }
}
