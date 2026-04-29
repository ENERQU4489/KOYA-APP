using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CloseWindowAction : IStreamDeckAction
    {
        public string Name => "Zamknij Okno";
        public string Description => "Symuluje Alt+F4 (zamyka aktywne okno)";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_MENU = 0x12; // Alt
        private const byte VK_F4 = 0x73;   // F4
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void Execute()
        {
            keybd_event(VK_MENU, 0, 0, 0); // Alt Down
            keybd_event(VK_F4, 0, 0, 0);   // F4 Down
            keybd_event(VK_F4, 0, KEYEVENTF_KEYUP, 0); // F4 Up
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0); // Alt Up
        }
    }
}
