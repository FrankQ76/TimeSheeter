using System;
using System.Windows.Forms;

namespace JsonFixer
{
    public static class ConsolLog
    {

        public static void ShowConsoleLog(ContextMenuStrip contextMenu)
        {

            GlobalParm._logging = !GlobalParm._logging;

            if (GlobalParm._logging)
            {
                GlobalParm._textLogging = "✔ " + GlobalParm._CstLogging;
            }
            else
            {
                GlobalParm._textLogging = GlobalParm._CstLogging;
            }

            SetConsoleWindowVisibility(GlobalParm._logging);
            ContextMenu.SetContextMenu(contextMenu);

            if (GlobalParm._logging)
            {
                Console.WriteLine($"Show Console Log : {GlobalParm._logging}.");
            }


        }
    }
}