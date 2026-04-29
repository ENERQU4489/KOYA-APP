using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class PasteAction : IStreamDeckAction
    {
        public string Name => "Wklej";
        public string Description => "Symuluje skrót klawiszowy Ctrl + V (Wklejanie).";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            const byte VK_CONTROL = 0x11;
            const byte VK_V = 0x56;
            const uint KEYEVENTF_KEYUP = 0x02;

            // Symulacja: CTRL w dół -> V w dół -> V w górę -> CTRL w górę
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_V, 0, 0, 0);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}