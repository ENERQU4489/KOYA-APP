using System.Runtime.InteropServices;
namespace KOYA_APP
{
    public class VolumeAction : IStreamDeckAction
    {
        public string Name => "Glosnosc +/-";
        public string Description => "Pozwala sterowac glosnoscia systemowa.";
        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        public void Execute() { } // Puste dla przycisku, uzywane przez ChangeVolume
        public void ChangeVolume(bool increase)
        {
            byte key = increase ? (byte)0xAF : (byte)0xAE;
            keybd_event(key, 0, 0, 0); keybd_event(key, 0, 2, 0);
        }
    }
}