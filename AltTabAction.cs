using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class AltTabAction : IStreamDeckAction
    {
        public string Name => "Przelacz Okno";
        public string Description => "Szybki Alt+Tab (przelacza na poprzednie okno)";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_MENU = 0x12; // Alt
        private const byte VK_TAB = 0x09;  // Tab
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void Execute()
        {
            keybd_event(VK_MENU, 0, 0, 0); // Alt Down
            keybd_event(VK_TAB, 0, 0, 0);  // Tab Down
            keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, 0); // Tab Up
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0); // Alt Up
        }
    }
}
