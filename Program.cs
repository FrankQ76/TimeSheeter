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
using System.Runtime.CompilerServices;
using System.Globalization;

//   _______
//   |/      |
//   |       |
//   |      ( )
//   |      /|\ _TIME SHEET 
//   |      / \ 
//   |
// __|___




namespace TimeSheeter
{

    static class GlobalParm
    {
        public static bool timerToggle = true;
        public static bool _ShowConsole = false;
        public static NotifyIcon notifyIcon = new NotifyIcon();
        public static string filePath = @"C:\LOCAL\TimeSheeter\time.txt";
        
        // Create a dictionary to store the total working time for each day
        public static Dictionary<DateTime, TimeSpan> workingTimeByDay = new Dictionary<DateTime, TimeSpan>();
        public static DateTime currentWeekStartDate = GetLastSaturday();
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
            ReadTimesheet();

            GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
            ExternalDll.SendMessage(Process.GetCurrentProcess().MainWindowHandle, ExternalDll.WM_SYSCOMMAND, ExternalDll.SC_MINIMIZE, 0);

            ExternalDll.SetConsoleWindowVisibility(GlobalParm._ShowConsole);

            GlobalParm.notifyIcon.Visible = true;
            GlobalParm.notifyIcon.Text = System.Windows.Forms.Application.ProductName;

            SetContextMenu();

            System.Windows.Forms.Application.Run();
            GlobalParm.notifyIcon.Visible = false;

            ExternalDll.FreeConsole();

            if (GlobalParm.timerToggle)
            {
                EndTimeSheet();
            }

        }

        private static void SetContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            ContextMenu.SetContextMenu(contextMenu);

            GlobalParm.notifyIcon.ContextMenuStrip = contextMenu;
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





        public static void ShowFileTimeSheet()
        {
            string fileName = GlobalParm.filePath; // replace with your file name
            string notepadPath = @"C:\Windows\System32\notepad.exe"; // path to Notepad.exe on your system

            Process.Start(notepadPath, fileName);

        }

        public static void ForceReloadTimesheet()
        {
            GlobalParm.workingTimeByDay.Clear(); 
            ReadTimesheet();
            SetContextMenu();
        }

            

        public static void ReadTimesheet()
        {

            string[] lines = File.ReadAllLines(GlobalParm.filePath, System.Text.Encoding.UTF8).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            DateTime startingDate = GlobalParm.currentWeekStartDate;



            // Loop through each line in the timesheet that starts from last saturday
            var filteredLines = lines
                .Where(line => DateTime.Parse(line.Split()[0]).Date >= startingDate);

            foreach (string line in filteredLines)
            {
                // Parse the start and end times from the line
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime startTime = DateTime.Parse(parts[0] + " " + parts[1]);
                DateTime endTime = DateTime.Parse(parts[3] + " " + parts[4]);

                // Calculate the working time for the line
                TimeSpan workingTime = endTime - startTime;

                // Handle time overlap at 23:59 and start the next day at 00:01
                if (endTime.TimeOfDay == new TimeSpan(23, 59, 0))
                {
                    endTime = endTime.AddMinutes(2);
                }

                // Add the working time to the total for the day
                DateTime date = startTime.Date;
                if (!GlobalParm.workingTimeByDay.ContainsKey(date))
                {
                    GlobalParm.workingTimeByDay[date] = TimeSpan.Zero;
                }
                GlobalParm.workingTimeByDay[date] += workingTime;
            }

            TimeSpan totalWorkedHours = totalWorkedHoursForTheWeek();

            foreach (var workingTime in GlobalParm.workingTimeByDay)
            {
                Console.WriteLine(workingTime.Key.ToString("yyyy-MM-dd") + " " + workingTime.Value.ToString());
            }

            // Output the total worked hours for each day and for the week
            Console.WriteLine("\nTotal Worked Hours Since Last saturday: " + totalWorkedHours.ToString());
        }

        private static DateTime GetLastSaturday()
        {
            DateTime today = DateTime.Today;
            int daysSinceLastSaturday = ((int)today.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
            DateTime lastSaturday = today.AddDays(-daysSinceLastSaturday);
            return lastSaturday;
        }


        public static TimeSpan totalWorkedHoursForTheWeek()
        {
            // Calculate the sum of worked hours for the week
            TimeSpan totalWorkedHours = TimeSpan.Zero;
            foreach (var workingTime in GlobalParm.workingTimeByDay)
            {
                totalWorkedHours += workingTime.Value;
            }

            return totalWorkedHours;
        }




        private static void EndTimeSheet()
        {
            // Stop tracking time
            endTime = DateTime.Now;

            // Handle time overlap at 23:59 and start the next day at 00:01
            if (endTime.TimeOfDay == new TimeSpan(23, 59, 0))
            {
                endTime = endTime.AddMinutes(2);
            }
                        
            using (StreamWriter writer = new StreamWriter(GlobalParm.filePath, true))
            {
                writer.WriteLine(startTime.ToString() + " to " + endTime.ToString());
            }

        }

        public static void NavigateToPreviousWeek()
        {
            GlobalParm.currentWeekStartDate = GlobalParm.currentWeekStartDate.AddDays(-7);
            ForceReloadTimesheet();
        }

        public static void NavigateToNextWeek()
        {
            GlobalParm.currentWeekStartDate = GlobalParm.currentWeekStartDate.AddDays(7);
            ForceReloadTimesheet();
        }

 
    
    }
}
