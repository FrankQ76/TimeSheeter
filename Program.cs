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

        public static NotifyIcon notifyIcon = new NotifyIcon();

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        
        public static IntPtr _hookID = IntPtr.Zero;

    }




    public class Program
    {


        public static LowLevelKeyboardProc _proc = HookCallback;


        [STAThread]
        public static void Main()
        {
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

        }

        public static void GetJSonPathValue(ContextMenuStrip contextMenu)
        {


            if (GlobalParm._logging)
            {
                Console.WriteLine($"\nFunction Get Json Path values requested.");
            }
                        
            var pathValuesTmp = ExtractAllJsonPath();
            

            if (!string.IsNullOrEmpty(pathValuesTmp))
            {
                var pathValues = pathValuesTmp.Remove(pathValuesTmp.Trim().Length - 1, 1);
                ActionClipboard.SetClip(pathValues);

                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nJson Path values has been copied to Clipboard.");
                }
            }
            else
            {
                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nClipboard is null or empty.");
                }
            }
            

        }




        // ================================





        

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)GlobalParm.WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);


                // CTRL-C  ( COPY function hook )
                if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    if (GlobalParm._logging)
                    {
                        Console.WriteLine($"\nCopy function activated ( CTRL-C ).");

                    }
                    GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                    System.Threading.Thread.Sleep(GlobalParm._sleepTimeMs);

                    var clipText = ActionClipboard.GetClipBoard();

                    if (!string.IsNullOrWhiteSpace(clipText))
                    {

                        try
                        {

                            var jsonDoc = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                            if (jsonDoc == clipText)
                            {
                                if (GlobalParm._logging)
                                {
                                    Console.WriteLine($"Json in clipboard is already in a good format.");

                                }
                                GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");

                            }
                            else
                            {
                                if (GlobalParm._logging)
                                {
                                    Console.WriteLine($"Json in clipboard is not well formated but is usable.");

                                }
                                GlobalParm.notifyIcon.Icon = new Icon("icon_red.ico");

                                if (GlobalParm._autoFix)
                                {
                                    ActionClipboard.SetClip(jsonDoc);
                                    if (GlobalParm._logging)
                                    {
                                        Console.WriteLine($"Autofixing Json in clipboard Done.");

                                    }

                                }

                            }

                        }
                        catch 
                        {
                            if (GlobalParm._logging)
                            {
                                Console.WriteLine($"Error occured during json parsing of clipboard value. Probably not json content.");

                            }
                            GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                        }
                    }
                    else
                    {
                        if (GlobalParm._logging)
                        {
                            Console.WriteLine($"Empty or locked clipboard.");

                        }

                        GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                    }


                }


                // CTRL-J   ( format JSON string )
                if (Keys.J == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    if (GlobalParm._logging)
                    {
                        Console.WriteLine($"\nForce Json formating function activated ( CTRL-J ).");

                    }
                    GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");

                    System.Threading.Thread.Sleep(GlobalParm._sleepTimeMs);

                    var clipText = ActionClipboard.GetClipBoard();

                    if (!string.IsNullOrWhiteSpace(clipText))
                    {

                        try
                        {

                            string jsonFormatted = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                            if (jsonFormatted == clipText)
                            {
                                if (GlobalParm._logging)
                                {
                                    Console.WriteLine($"Json in clipboard is already in a good format.");

                                }

                                GlobalParm.notifyIcon.Icon = new Icon("icon_green.ico");

                            }
                            else
                            {
                                ActionClipboard.SetClip(jsonFormatted);
                                if (GlobalParm._logging)
                                {
                                    Console.WriteLine($"Fixing Json in clipboard Done.");

                                }
                                
                                

                            }
                        }
                        catch
                        {
                            if (GlobalParm._logging)
                            {
                                Console.WriteLine($"Error occured during json parsing of clipboard value. Probably not json content.");

                            }
                            GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                        }

                    }
                    else
                    {
                        if (GlobalParm._logging)
                        {
                            Console.WriteLine($"Empty or locked clipboard.");

                        }

                        GlobalParm.notifyIcon.Icon = new Icon("icon_gray.ico");
                    }

                }
            }
            return ExternalDll.CallNextHookEx(GlobalParm._hookID, nCode, wParam, lParam);
        }

        public static bool IsJsonValid(string json)
        {

            try
            {
                //var clipText = GetClipBoard();
                var jsonFormatted = JValue.Parse(json).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
                return true;
            }
            catch
            {
                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nNot a valid json document in the Clipboard.");

                }
                return false;
            }
        }



        public static string ExtractAllJsonPath()
        {
            var json = ActionClipboard.GetClipBoard();


            if (!string.IsNullOrWhiteSpace(json) && IsJsonValid(json))
            {
                var jobject = JObject.Parse(json);
                var sb = new StringBuilder();

                RecursiveParse(sb, jobject);
                                
                if (GlobalParm._logging)
                {
                    Console.WriteLine("\nAll the Json Path values ready to be use in SQL: \n");
                    Console.WriteLine(sb.ToString());
                }

                return sb.ToString();
            }

            if (GlobalParm._logging)
            {
                Console.WriteLine("\nCannot extract path values.");
            }

            return string.Empty;

        }


        public static void RecursiveParse(StringBuilder sb, JToken token)
        {
            foreach (var item in token.Children())
            {
                if (item.HasValues)
                {
                    RecursiveParse(sb, item);
                }
                else
                {
                    sb.AppendLine($"JSON_VALUE(Data, '$.{item.Path}') as {item.Path.Split('.').Last()}," );
                }
            }

        }




        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return ExternalDll.SetWindowsHookEx(GlobalParm.WH_KEYBOARD_LL, _proc, ExternalDll.GetModuleHandle(curModule.ModuleName), 0);
            }
        }



    }
}
