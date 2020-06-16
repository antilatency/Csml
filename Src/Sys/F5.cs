using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
namespace Csml {
    class F5 {
        public static List<string> ProcessNames = new List<string>() {
            "chrome",
            "iexplore",
            "opera"
        };

        public static string Prefix = "f5me.";

        public static void Send() {

            var processes = Process.GetProcesses();
            var currentForegroundWindow = GetForegroundWindow();
            foreach (var p in processes) {
                if (!ProcessNames.Contains(p.ProcessName)) continue;
                var h = p.MainWindowHandle;
                if (h != IntPtr.Zero) {
                    var text = GetWindowText(h);
                    if (text.StartsWith(Prefix)) {
                        Console.WriteLine(p.ProcessName);

                        SetForegroundWindow(h);
                        uint KEYEVENTF_KEYUP = 0x0002;
                        keybd_event((byte)VK_F5, 0xbf, 0, UIntPtr.Zero);
                        keybd_event((byte)VK_F5, 0xbf, KEYEVENTF_KEYUP, UIntPtr.Zero);

                        //SendMessage(h, WM_SYSKEYDOWN, VK_F5, 0);
                        //SendMessage(h, WM_SYSKEYUP, VK_F5, 0);
                    }
                }
            }
            SetForegroundWindow(currentForegroundWindow);
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static readonly int VK_F5 = 0x74;

        /*private static readonly uint WM_SYSKEYDOWN = 0x0104;
        private static readonly uint WM_SYSKEYUP = 0x0105;       

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);*/

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageStringBuilder(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static string GetWindowText(IntPtr handle) {
            const uint WM_GETTEXT = 0x000D;
            StringBuilder message = new StringBuilder(1000);
            SendMessageStringBuilder(handle, WM_GETTEXT, message.Capacity, message);
            return message.ToString();
        }

    }
}