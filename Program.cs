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

namespace JsonFixer
{

    internal class Program
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool _autoFix = false;
        private static bool _logging = false;
        private static int _maxRetries = 5;
        private static int _sleepTimeMs = 800;

        private static string _CstAutoFix = "Set AutoFix";
        private static string _CstLogging = "Show Console Log";

        private static string _textAutoFix = _CstAutoFix;
        private static string _textLogging = _CstLogging;


        static NotifyIcon notifyIcon = new NotifyIcon();


        [STAThread]
        public static void Main()
        {
            notifyIcon.Icon = new Icon("icon_gray.ico");
            SendMessage(Process.GetCurrentProcess().MainWindowHandle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
            _hookID = SetHook(_proc);
            SetConsoleWindowVisibility(_logging);


            notifyIcon.Visible = true;
            notifyIcon.Text = System.Windows.Forms.Application.ProductName;

            var contextMenu = new ContextMenuStrip();

            SetContextMenu(contextMenu);

            notifyIcon.ContextMenuStrip = contextMenu;

            System.Windows.Forms.Application.Run();
            notifyIcon.Visible = false;
            UnhookWindowsHookEx(_hookID);
            FreeConsole();

        }

        private static void SetContextMenu(ContextMenuStrip contextMenu)
        {
            contextMenu.Items.Clear();

            contextMenu.Items.Add("Exit", null, (s, e) => { System.Windows.Forms.Application.Exit(); });

            contextMenu.Items.Add("Get Json Path Values", null, onClick: (s, e) => { GetJSonPathValue(contextMenu); });

            contextMenu.Items.Add(_textLogging, null, onClick: (s, e) => { ShowConsoleLog(contextMenu); });

            contextMenu.Items.Add(_textAutoFix, null, onClick: (s, e) => { SetAutoFix(contextMenu); });
        }

        private static void SetAutoFix(ContextMenuStrip contextMenu)
        {

            _autoFix = !_autoFix;

            if (_autoFix)
            {
                _textAutoFix = "✔ " + _CstAutoFix;
            }
            else
            {
                _textAutoFix = _CstAutoFix;
            }

            SetContextMenu(contextMenu);

            if (_logging)
            {
                Console.WriteLine($"AutoFix : {_autoFix}.");
            }


        }

        private static void ShowConsoleLog(ContextMenuStrip contextMenu)
        {

            _logging = !_logging;

            if (_logging)
            {
                _textLogging = "✔ " + _CstLogging;
            }
            else
            {
                _textLogging = _CstLogging;
            }

            SetConsoleWindowVisibility(_logging);

            SetContextMenu(contextMenu);

            if (_logging)
            {
                Console.WriteLine($"Show Console Log : {_logging}.");
            }


        }

        private static void GetJSonPathValue(ContextMenuStrip contextMenu)
        {


            if (_logging)
            {
                Console.WriteLine($"\nFunction Get Json Path values requested.");
            }
                        
            var pathValuesTmp = ExtractAllJsonPath();
            

            if (!string.IsNullOrEmpty(pathValuesTmp))
            {
                var pathValues = pathValuesTmp.Remove(pathValuesTmp.Trim().Length - 1, 1);
                SetClip(pathValues);

                if (_logging)
                {
                    Console.WriteLine($"\nJson Path values has been copied to Clipboard.");
                }
            }
            else
            {
                if (_logging)
                {
                    Console.WriteLine($"\nClipboard is null or empty.");
                }
            }
            

        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);



        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);


                // CTRL-C  ( COPY function hook )
                if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    if (_logging)
                    {
                        Console.WriteLine($"\nCopy function activated ( CTRL-C ).");

                    }
                    notifyIcon.Icon = new Icon("icon_gray.ico");

                    System.Threading.Thread.Sleep(_sleepTimeMs);

                    var clipText = GetClipBoard();

                    if (!string.IsNullOrWhiteSpace(clipText))
                    {

                        try
                        {

                            var jsonDoc = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                            if (jsonDoc == clipText)
                            {
                                if (_logging)
                                {
                                    Console.WriteLine($"Json in clipboard is already in a good format.");

                                }
                                notifyIcon.Icon = new Icon("icon_green.ico");

                            }
                            else
                            {
                                if (_logging)
                                {
                                    Console.WriteLine($"Json in clipboard is not well formated but is usable.");

                                }
                                notifyIcon.Icon = new Icon("icon_red.ico");

                                if (_autoFix)
                                {
                                    SetClip(jsonDoc);
                                    if (_logging)
                                    {
                                        Console.WriteLine($"Autofixing Json in clipboard Done.");

                                    }

                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            if (_logging)
                            {
                                Console.WriteLine($"Error occured during json parsing of clipboard value. Probably not json content.");

                            }
                            notifyIcon.Icon = new Icon("icon_gray.ico");

                        }
                    }
                    else
                    {
                        if (_logging)
                        {
                            Console.WriteLine($"Empty or locked clipboard.");

                        }

                        notifyIcon.Icon = new Icon("icon_gray.ico");
                    }


                }


                // CTRL-J   ( format JSON string )
                if (Keys.J == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    if (_logging)
                    {
                        Console.WriteLine($"\nForce Json formating function activated ( CTRL-J ).");

                    }
                    notifyIcon.Icon = new Icon("icon_gray.ico");

                    System.Threading.Thread.Sleep(_sleepTimeMs);

                    var clipText = GetClipBoard();

                    if (!string.IsNullOrWhiteSpace(clipText))
                    {

                        try
                        {

                            string jsonFormatted = JValue.Parse(clipText).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);

                            if (jsonFormatted == clipText)
                            {
                                if (_logging)
                                {
                                    Console.WriteLine($"Json in clipboard is already in a good format.");

                                }

                                notifyIcon.Icon = new Icon("icon_green.ico");

                            }
                            else
                            {
                                SetClip(jsonFormatted);
                                if (_logging)
                                {
                                    Console.WriteLine($"Fixing Json in clipboard Done.");

                                }
                                
                                

                            }
                        }
                        catch (Exception ex)
                        {
                            if (_logging)
                            {
                                Console.WriteLine($"Error occured during json parsing of clipboard value. Probably not json content.");

                            }
                            notifyIcon.Icon = new Icon("icon_gray.ico");
                        }

                    }
                    else
                    {
                        if (_logging)
                        {
                            Console.WriteLine($"Empty or locked clipboard.");

                        }

                        notifyIcon.Icon = new Icon("icon_gray.ico");
                    }

                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void SetClip(string jsonFormatted)
        {
            Clipboard.Clear();

            System.Threading.Thread.Sleep(60);

            Clipboard.SetDataObject(jsonFormatted, true, 30, 40);

            notifyIcon.Icon = new Icon("icon_green.ico");

        }

        private static string GetClipBoard()
        {

            try
            {
                var contents = GetClipTextThreaded();
                return contents;
            }
            catch { }
            {
                if (_logging)
                {
                    Console.WriteLine($"Clipboard is locked by a process.");

                }

                return string.Empty;
            }


        }

        private static bool IsJsonValid(string json)
        {

            try
            {
                //var clipText = GetClipBoard();
                var jsonFormatted = JValue.Parse(json).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
                return true;
            }
            catch
            {
                if (_logging)
                {
                    Console.WriteLine($"\nNot a valid json document in the Clipboard.");

                }
                return false;
            }
        }

        public static string GetClipTextThreaded()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    if (System.Windows.Forms.Clipboard.ContainsText())
                    {
                        ReturnValue = System.Windows.Forms.Clipboard.GetText();
                    }

                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }

        public static string ExtractAllJsonPath()
        {
            var json = GetClipBoard();


            if (!string.IsNullOrWhiteSpace(json) && IsJsonValid(json))
            {
                var jobject = JObject.Parse(json);
                var sb = new StringBuilder();

                RecursiveParse(sb, jobject);
                                
                if (_logging)
                {
                    Console.WriteLine("\nAll the Json Path values ready to be use in SQL: \n");
                    Console.WriteLine(sb.ToString());
                }

                return sb.ToString();
            }

            if (_logging)
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


        //==============================================================================================


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        internal static extern bool SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);
        static Int32 WM_SYSCOMMAND = 0x0112;
        static Int32 SC_MINIMIZE = 0x0F020;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1); //1 = SW_SHOWNORMAL           
                else ShowWindow(hWnd, 0); //0 = SW_HIDE               
            }
        }

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean AttachConsole(Int32 ProcessId);

        // Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        // Also consider whether you're being lazy or not.
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);


    }
}
