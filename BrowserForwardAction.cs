using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class BrowserForwardAction : IStreamDeckAction
    {
        public string Name => "Dalej (WWW)";
        public string Icon => "\uE72A";
        public string Category => "Internet & Aplikacje";
        public string Description => "Następna strona w przeglądarce";

        public void Execute()
        {
            keybd_event(0xA7, 0, 0, 0);
            keybd_event(0xA7, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
