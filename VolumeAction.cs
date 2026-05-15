using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class VolumeAction : IStreamDeckAction
    {
        public string Name => "Glosnosc";
        public string Icon => "\uE15D";
        public string Description => "Glowna glosnosc systemu";

        public void Execute() { keybd_event(0xAD, 0, 0, 0); keybd_event(0xAD, 0, 2, 0); }
        public void ExecuteAnalog(bool direction) { byte vk = direction ? (byte)0xAF : (byte)0xAE; keybd_event(vk, 0, 0, 0); keybd_event(vk, 0, 2, 0); }

        [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
