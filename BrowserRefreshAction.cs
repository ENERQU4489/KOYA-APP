using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class BrowserRefreshAction : IStreamDeckAction
    {
        public string Name => "Odśwież (WWW)";
        public string Icon => "\uE72C";
        public string Description => "Odśwież stronę w przeglądarce";

        public void Execute()
        {
            keybd_event(0xA8, 0, 0, 0);
            keybd_event(0xA8, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
