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
using static JsonFixer.Program;
using Newtonsoft.Json.Bson;
using System.Timers;
using System.IO;

namespace JsonFixer
{

    static class GlobalParm
    {
        public static bool _autoFix = false;
        public static bool _logging = false;
        public static int _maxRetries = 5;
        public static int _sleepTimeMs = 800;
        
        public static string _CstAutoFix = "Set AutoFix";
        public static string _CstLogging = "Show Console Log";
        public static string _textAutoFix = _CstAutoFix;
        public static string _textLogging = _CstLogging;
        
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public static IntPtr _hookID = IntPtr.Zero;
        public static NotifyIcon notifyIcon = new NotifyIcon();
    }

    // ================================


    public class Program
    {


        private static LowLevelKeyboardProc _proc = HookCallback;
        private static DateTime startTime;
        private static DateTime endTime;


        [STAThread]
        internal static void Main()
        {
            StartTimeSheet();

            GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
            ExternalDll.SendMessage(Process.GetCurrentProcess().MainWindowHandle, ExternalDll.WM_SYSCOMMAND, ExternalDll.SC_MINIMIZE, 0);
            GlobalParm._hookID = SetHook(_proc);
            ExternalDll.SetConsoleWindowVisibility(GlobalParm._logging);


            GlobalParm.notifyIcon.Visible = true;
            GlobalParm.notifyIcon.Text = System.Windows.Forms.Application.ProductName;

            var contextMenu = new ContextMenuStrip();
            ContextMenu.SetContextMenu(contextMenu);

            GlobalParm.notifyIcon.ContextMenuStrip = contextMenu;

            System.Windows.Forms.Application.Run();
            GlobalParm.notifyIcon.Visible = false;
            ExternalDll.UnhookWindowsHookEx(GlobalParm._hookID);
            ExternalDll.FreeConsole();

            EndTimeSheet();

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
            string filePath = @"C:\LOCAL\JsonFixer\time_detail.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(endTime.ToString() + " - " + startTime.ToString() + " = " + timeValue.ToString());
            }
            
            filePath = @"C:\LOCAL\JsonFixer\time.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(startTime.ToString() + " to " + endTime.ToString());
            }

        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return ExternalDll.SetWindowsHookEx(GlobalParm.WH_KEYBOARD_LL, proc, ExternalDll.GetModuleHandle(curModule.ModuleName), 0);
            }
        }


        // =======================================
        //  Define what to do on key combinations
        // =======================================
        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)GlobalParm.WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);


                CTRL_C(vkCode); // CTRL-C  ( COPY function hook )
                                
                CTRL_JSONFIX(vkCode); // CTRL-J   ( format JSON string )

            }

            return ExternalDll.CallNextHookEx(GlobalParm._hookID, nCode, wParam, lParam);
        }



        private static void CTRL_C(int vkCode)
        {
            
            if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
            {
                Write2Log.WriteToLog("\nCopy function activated ( CTRL-C ).");

                GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                Thread.Sleep(GlobalParm._sleepTimeMs);

                var clipText = ActionClipboard.GetClipBoard();

                if (!string.IsNullOrWhiteSpace(clipText))
                {

                    try
                    {

                        var jsonDoc = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                        if (jsonDoc == clipText)
                        {
                            Write2Log.WriteToLog($"Json in clipboard is already in a good format.");
                            GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
                        }
                        else
                        {
                            Write2Log.WriteToLog($"Json in clipboard is not well formated but is usable.");
                            GlobalParm.notifyIcon.Icon = new Icon("icon_red.ico");
                            if (GlobalParm._autoFix)
                            {
                                ActionClipboard.SetClip(jsonDoc);
                                Write2Log.WriteToLog($"Autofixing Json in clipboard Done.");
                                GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
                            }
                        }

                    }
                    catch
                    {
                        Write2Log.WriteToLog($"Error occured during json parsing of clipboard value. Probably not json content.");
                        GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                    }
                }
                else
                {
                    Write2Log.WriteToLog($"Empty or locked clipboard.");
                    GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                }


            }
        }


        // =======================================

        private static void CTRL_JSONFIX(int vkCode)
        {
            // CTRL-J   ( format JSON string )
            if (Keys.J == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
            {
                Write2Log.WriteToLog($"\nForce Json formating function activated ( CTRL-J ).");

                GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                Thread.Sleep(GlobalParm._sleepTimeMs);

                var clipText = ActionClipboard.GetClipBoard();

                if (!string.IsNullOrWhiteSpace(clipText))
                {

                    try
                    {

                        string jsonFormatted = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                        if (jsonFormatted == clipText)
                        {
                            Write2Log.WriteToLog($"Json in clipboard is already in a good format.");
                            GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");
                        }
                        else
                        {
                            ActionClipboard.SetClip(jsonFormatted);
                            Write2Log.WriteToLog($"Fixing Json in clipboard Done.");
                        }
                    }
                    catch
                    {
                        Write2Log.WriteToLog($"Error occured during json parsing of clipboard value. Probably not json content.");
                        GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                    }

                }
                else
                {
                    Write2Log.WriteToLog($"Empty or locked clipboard.");
                    GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                }

            }
        }
    }
}
