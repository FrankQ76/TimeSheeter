using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace JsonFixer
{
    public class AutoFix
    {

        public static void SetAutoFix(ContextMenuStrip contextMenu)
        {


            GlobalParm._autoFix = !GlobalParm._autoFix;

            if (GlobalParm._autoFix)
            {
                GlobalParm._textAutoFix = "✔ " + GlobalParm._CstAutoFix;
            }
            else
            {
                GlobalParm._textAutoFix = GlobalParm._CstAutoFix;
            }

            ContextMenu.SetContextMenu(contextMenu);

            if (GlobalParm._logging)
            {
                Console.WriteLine($"AutoFix : {GlobalParm._autoFix}.");
            }


        }
    }
}