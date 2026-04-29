using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CopyAction : IStreamDeckAction
    {
        public string Name => "Kopiuj";
        public string Description => "Symuluje skrót klawiszowy Ctrl + C (Kopiowanie).";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            const byte VK_CONTROL = 0x11;
            const byte VK_C = 0x43;
            const uint KEYEVENTF_KEYUP = 0x02;

            // Symulacja: CTRL w dół -> C w dół -> C w górę -> CTRL w górę
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_C, 0, 0, 0);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}