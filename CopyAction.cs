using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CopyAction : IStreamDeckAction
    {
        public string Name => "Skopiuj";
        public string Icon => "\uE16F";
        public string Category => "System & Okna";
        public string Description => "Kopiowanie systemowe (Ctrl+C)";

        public void Execute() { keybd_event(0x11, 0, 0, 0); keybd_event(0x43, 0, 0, 0); keybd_event(0x43, 0, 2, 0); keybd_event(0x11, 0, 2, 0); }
        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }

        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
