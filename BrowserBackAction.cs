using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class BrowserBackAction : IStreamDeckAction
    {
        public string Name => "Wstecz (WWW)";
        public string Icon => "\uE72B";
        public string Category => "Internet & Aplikacje";
        public string Description => "Poprzednia strona w przeglądarce";

        public void Execute()
        {
            keybd_event(0xA6, 0, 0, 0);
            keybd_event(0xA6, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
