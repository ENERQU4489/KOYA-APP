using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class PasteAction : IStreamDeckAction
    {
        public string Name => "Wklej";
        public string Icon => "\uE16D";
        public string Description => "Wklejanie systemowe (Ctrl+V)";

        public void Execute() { keybd_event(0x11, 0, 0, 0); keybd_event(0x56, 0, 0, 0); keybd_event(0x56, 0, 2, 0); keybd_event(0x11, 0, 2, 0); }
        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
