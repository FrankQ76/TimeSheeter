using System.Windows.Forms;

namespace JsonFixer
{
    public static class ContextMenu
    {

        public static void SetContextMenu(ContextMenuStrip contextMenu)
        {
            contextMenu.Items.Clear();

            contextMenu.Items.Add("Removes Duplicates Values", null, onClick: (s, e) => { Duplicates.RemoveDuplicates(contextMenu); });

            contextMenu.Items.Add("Get Duplicates Only", null, onClick: (s, e) => { Duplicates.GetDuplicates(contextMenu); });

            contextMenu.Items.Add("Get Json Path Values", null, onClick: (s, e) => { GetJSonPathValue(contextMenu); });

            contextMenu.Items.Add(GlobalParm._textLogging, null, onClick: (s, e) => { ConsolLog.ShowConsoleLog(contextMenu); });

            contextMenu.Items.Add(GlobalParm._textAutoFix, null, onClick: (s, e) => { AutoFix.SetAutoFix(contextMenu); });

            contextMenu.Items.Add("Exit", null, (s, e) => { System.Windows.Forms.Application.Exit(); });


        }
    }
}