using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LaserPointerServer
{
    /** Update later to use a more modern/C#-ish interface
    */
    public class KeyboardAndMouseSender
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 cButtons, IntPtr dwExtraInfo);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, IntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetCursorPos(UInt32 X, UInt32 Y);

        private UInt32 MOUSEEVENTF_LEFTDOWN = 2;
        private UInt32 MOUSEEVENTF_LEFTUP = 4;        
        private UInt32 KEYEVENTF_KEYUP = 0x0002;

        //private byte VK_SHIFT = 0x10;
        private byte VK_CONTROL = 0x11;
        private byte VK_NEXT = 0x22;
        private byte VK_PRIOR = 0x21;

        public void SetCursor(uint x, uint y) {
            SetCursorPos(x, y);
        }

        public void SetCursorLeftDown(uint x, uint y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, IntPtr.Zero);
        }

        public void SetCursorLeftUp(uint x, uint y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, IntPtr.Zero);
        }

        public void SetCtrlDown() {
            keybd_event(VK_CONTROL, 0, 0, IntPtr.Zero);
        }

        public void SetCtrlUp() {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        public void SendPgDown() {
            keybd_event(VK_NEXT, 0, 0, IntPtr.Zero);
            Thread.Sleep(100);
            keybd_event(VK_NEXT, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        public void SendPgUp()
        {
            keybd_event(VK_PRIOR, 0, 0, IntPtr.Zero);
            Thread.Sleep(100);
            keybd_event(VK_PRIOR, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }            
    }
}
