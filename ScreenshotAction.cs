using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class ScreenshotAction : IStreamDeckAction
    {
        public string Name => "Zrzut Ekranu";
        public string Icon => "\uE158";
        public string Category => "System & Okna";
        public string Description => "Wykonuje zrzut calego ekranu";

        public void Execute()
        {
            keybd_event(0x2C, 0, 0, 0);
            keybd_event(0x2C, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
