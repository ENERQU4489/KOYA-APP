using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class WebZoomAction : IStreamDeckAction
    {
        public string Name => "Powiekszenie";
        public string Icon => "\uE1A3";
        public string Description => "Kontrola powiekszenia (Zoom)";

        public void Execute() { keybd_event(0x11, 0, 0, 0); keybd_event(0x30, 0, 0, 0); keybd_event(0x30, 0, 2, 0); keybd_event(0x11, 0, 2, 0); }
        public void ExecuteAnalog(bool direction) { keybd_event(0x11, 0, 0, 0); uint d = direction ? (uint)120 : unchecked((uint)-120); mouse_event(0x0800, 0, 0, d, 0); keybd_event(0x11, 0, 2, 0); }

        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")] private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
    }
}
