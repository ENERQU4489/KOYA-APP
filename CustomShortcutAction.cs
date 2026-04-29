using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KOYA_APP
{
    public class CustomShortcutAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Skrot klawiszowy";
        public string Description => $"Wykonuje: {KeysDisplay}";
        public string KeysDisplay { get; set; } = "Nieustawiony";
        
        // Przechowujemy kody Virtual-Key
        public List<byte> KeyCodes { get; set; } = new List<byte>();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void Execute()
        {
            if (KeyCodes.Count == 0) return;

            // Wcisnij wszystkie klawisze w kolejnosci
            foreach (var key in KeyCodes)
            {
                keybd_event(key, 0, 0, 0);
            }

            // Pusc wszystkie klawisze w odwrotnej kolejnosci
            for (int i = KeyCodes.Count - 1; i >= 0; i--)
            {
                keybd_event(KeyCodes[i], 0, KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
