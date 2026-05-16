using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class FullscreenAction : IStreamDeckAction
    {
        public string Name => "Pelny Ekran";
        public string Icon => "\uE1D9";
        public string Category => "System & Okna";
        public string Description => "Klawisz F11";

        public void Execute()
        {
            keybd_event(0x7A, 0, 0, 0);
            keybd_event(0x7A, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
