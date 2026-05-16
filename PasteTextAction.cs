using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;

namespace KOYA_APP
{
    public class PasteTextAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Wklej Tekst";
        public string Description { get; set; } = "Wkleja zdefiniowany tekst w miejscu kursora.";
        public string Icon => "\uE77F;"; // Ikona notatki/tekstu
        public string Category => "Automatyzacja & Narzędzia";
        
        public string TextToPaste { get; set; } = "";

        public void Execute()
        {
            if (string.IsNullOrEmpty(TextToPaste)) return;

            Thread thread = new Thread(() =>
            {
                try
                {
                    // Ustawienie tekstu w schowku (wymaga wątku STA)
                    System.Windows.Clipboard.SetText(TextToPaste);
                    
                    // Symulacja Ctrl+V
                    keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Paste error: {ex.Message}");
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void ExecuteAnalog(bool direction) { /* Nie dotyczy przycisków */ }

        // WinAPI do symulacji klawiszy
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const uint KEYEVENTF_KEYUP = 0x0002;
    }
}
