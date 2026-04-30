using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CopyAction : IStreamDeckAction
    {
        public string Name => "COPY";
        public string Icon => "\uE16F";
        public string Description => "System Copy (Ctrl+C)";

        public void Execute() { keybd_event(0x11, 0, 0, 0); keybd_event(0x43, 0, 0, 0); keybd_event(0x43, 0, 2, 0); keybd_event(0x11, 0, 2, 0); }
        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
