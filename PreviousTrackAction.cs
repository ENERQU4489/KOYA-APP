using System.Runtime.InteropServices;
namespace KOYA_APP
{
    public class PreviousTrackAction : IStreamDeckAction
    {
        public string Name => "Poprzedni utwór";
        public string Description => "Wraca do poprzedniej piosenki.";
        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        public void Execute() { keybd_event(0xB1, 0, 0, 0); keybd_event(0xB1, 0, 2, 0); }
    }
}