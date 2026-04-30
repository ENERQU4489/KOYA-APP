using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CloseWindowAction : IStreamDeckAction
    {
        public string Name => "Zamknij Okno";
        public string Icon => "\uE10A";
        public string Description => "Alt+F4";

        public void Execute()
        {
            keybd_event(0x12, 0, 0, 0);
            keybd_event(0x73, 0, 0, 0);
            keybd_event(0x73, 0, 2, 0);
            keybd_event(0x12, 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
