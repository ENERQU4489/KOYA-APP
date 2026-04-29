using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class ScreenshotAction : IStreamDeckAction
    {
        public string Name => "Zrzut ekranu";
        public string Description => "Robi pelny zrzut ekranu (Win + PrintScreen)";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            const byte VK_LWIN = 0x5B;
            const byte VK_SNAPSHOT = 0x2C; // Klucz PrintScreen
            const uint KEYEVENTF_KEYUP = 0x02;

            // Sekwencja: Win w dól -> PrintScreen w dól -> PrintScreen w góre -> Win w góre
            keybd_event(VK_LWIN, 0, 0, 0);
            keybd_event(VK_SNAPSHOT, 0, 0, 0);
            keybd_event(VK_SNAPSHOT, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}