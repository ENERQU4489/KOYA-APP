using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class NextTrackAction : IStreamDeckAction
    {
        public string Name => "Nastepny Utwor";
        public string Icon => "\uE101";
        public string Category => "Multimedia & Audio";
        public string Description => "Przelacza utwor";

        public void Execute()
        {
            keybd_event(0xB0, 0, 0, 0);
            keybd_event(0xB0, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
