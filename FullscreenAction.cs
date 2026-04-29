using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class FullscreenAction : IStreamDeckAction
    {
        public string Name => "Pelny Ekran";
        public string Description => "Symuluje klawisz F11";

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_F11 = 0x7A;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void Execute()
        {
            keybd_event(VK_F11, 0, 0, 0);
            keybd_event(VK_F11, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
