using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HHZPlayer.SingleInstance
{
    public static class WindowActivator
    {
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        public static void BringToFront(Form f)
        {
            if (f.WindowState == FormWindowState.Minimized)
                f.WindowState = FormWindowState.Normal;
            ShowWindow(f.Handle, SW_RESTORE);
            f.Activate();
            SetForegroundWindow(f.Handle);
            // nudge
            f.TopMost = true; f.TopMost = false;
        }
    }
}
