using System;
using System.Windows.Forms;

namespace TimeSheeter
{
    public static class ContextMenu
    {
            

        public static void SetContextMenu(ContextMenuStrip contextMenu)
        {

            contextMenu.Items.Add("Show file TimeSheet", null, (s, e) => { Program.ShowFileTimeSheet(); });

            string separator       = "---------------";    
            string sunLabel = string.Empty;
            string monLabel = string.Empty;
            string tueLabel = string.Empty;
            string wedLabel = string.Empty;
            string thuLabel = string.Empty;
            string friLabel = string.Empty;
            string satLabel = string.Empty;
            string doubleseparator = "===============";
            string totalWeek = string.Empty;

            contextMenu.Items.Clear();

            contextMenu.Items.Add("Reload Timesheet", null, (s, e) => { Program.ForceReloadTimesheet(); });


            

            foreach (var workingTime in GlobalParm.workingTimeByDay)
            {
                switch (workingTime.Key.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        satLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Sunday:
                        sunLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Monday:
                        monLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Tuesday:
                        tueLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Wednesday:
                        wedLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Thursday:
                        thuLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;
                    case DayOfWeek.Friday:
                        friLabel = FormatWorkingTime(workingTime.Key, workingTime.Value);
                        break;

                    default:
                        break;
                }
            }



            // Create a new context menu for the "Show TimeSheet" item
            ContextMenuStrip showTimeSheetMenu = new ContextMenuStrip();


            if (!string.IsNullOrWhiteSpace(satLabel))
                showTimeSheetMenu.Items.Add(satLabel, null, (s, e) => { });
            
            if (!string.IsNullOrWhiteSpace(sunLabel))
                showTimeSheetMenu.Items.Add(sunLabel, null, (s, e) => { });
        
            showTimeSheetMenu.Items.Add(monLabel, null, (s, e) => { });
            showTimeSheetMenu.Items.Add(tueLabel, null, (s, e) => { });
            showTimeSheetMenu.Items.Add(wedLabel, null, (s, e) => { });
            showTimeSheetMenu.Items.Add(thuLabel, null, (s, e) => { });
            showTimeSheetMenu.Items.Add(friLabel, null, (s, e) => { });


            showTimeSheetMenu.Items.Add(doubleseparator, null, (s, e) => { });

           

            TimeSpan totalWorkedHours = Program.totalWorkedHoursForTheWeek();
            // Convert the total worked hours to decimal format
            
            decimal totalWorkedHoursDecimal = Math.Round((decimal)totalWorkedHours.TotalSeconds / 3600, 2);
            totalWeek = "Total Week = " + totalWorkedHoursDecimal.ToString();


            showTimeSheetMenu.Items.Add(totalWeek, null, (s, e) => { });


            ToolStripMenuItem showTimeSheetItem = new ToolStripMenuItem("Show TimeSheet", null, null, Keys.None);
            showTimeSheetItem.DropDown = showTimeSheetMenu;
            contextMenu.Items.Add(showTimeSheetItem);



            contextMenu.Items.Add("Toggle(on/off)", null, (s, e) => { Program.ToggleTimeSheet(); });

            contextMenu.Items.Add("Exit", null, (s, e) => { System.Windows.Forms.Application.Exit(); });
        }





        private static string FormatWorkingTime(DateTime date, TimeSpan workingTime)
        {
            // Handle time overlap at 23:59 and start the next day at 00:01
            if (workingTime == new TimeSpan(23, 59, 0))
            {
                workingTime = workingTime.Add(new TimeSpan(0, 2, 0));
            }

            return date.ToString("yyyy-MM-dd") + " = " + workingTime.ToString(@"hh\:mm\:ss");
        }




    }
}
