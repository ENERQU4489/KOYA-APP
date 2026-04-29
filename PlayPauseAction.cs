using System.Runtime.InteropServices;
namespace KOYA_APP
{
    public class PlayPauseAction : IStreamDeckAction
    {
        public string Name => "Play / Pause";
        public string Description => "Zatrzymuje lub wznawia muzyke.";
        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        public void Execute() { keybd_event(0xB3, 0, 0, 0); keybd_event(0xB3, 0, 2, 0); }
    }
}