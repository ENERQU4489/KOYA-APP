using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KOYA_APP
{
    public class CustomShortcutAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Skrot klawiszowy";
        public string Icon => "\uE144";
        public string Category => "Automatyzacja & Narzędzia";
        public string Description => $"Wykonuje: {KeysDisplay}";
        public string KeysDisplay { get; set; } = "Nieustawiony";
        
        public List<byte> KeyCodes { get; set; } = new List<byte>();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            if (KeyCodes.Count == 0) return;
            foreach (var key in KeyCodes) keybd_event(key, 0, 0, 0);
            for (int i = KeyCodes.Count - 1; i >= 0; i--) keybd_event(KeyCodes[i], 0, 2, 0);
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
