using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class AltTabAction : IStreamDeckAction
    {
        public string Name => "Przelacz Okno";
        public string Icon => "\uE117";
        public string Description => "Szybki Alt+Tab";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            keybd_event(0x12, 0, 0, 0); // Alt
            keybd_event(0x09, 0, 0, 0); // Tab
            keybd_event(0x09, 0, 2, 0);
            keybd_event(0x12, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
