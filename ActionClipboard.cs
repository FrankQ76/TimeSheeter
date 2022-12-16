using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace JsonFixer
{
    public static class ActionClipboard
    {
        private static string GetClipTextThreaded()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    if (Clipboard.ContainsText())
                    {
                        ReturnValue = Clipboard.GetText();
                    }

                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }

        public static string GetClipBoard()
        {

            try
            {
                var contents = GetClipTextThreaded();
                return contents;
            }
            catch
            {
                if (GlobalParm._logging)
                {
                    Console.WriteLine($"Clipboard is locked by a process.");

                }

                return string.Empty;
            }


        }

        public static void SetClip(string jsonFormatted)
        {
            Clipboard.Clear();

            System.Threading.Thread.Sleep(60);

            Clipboard.SetDataObject(jsonFormatted, true, 30, 40);

            GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");

        }
    }
}