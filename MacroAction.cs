using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KOYA_APP
{
    public class MacroAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Makro (Sekwencja)";
        public string Icon => "\uE7C8"; // Command prompt icon or similar
        public string Description => $"Sekwencja {Steps.Count} klawiszy";
        
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void Execute()
        {
            if (Steps.Count == 0) return;
            
            Task.Run(() =>
            {
                foreach (var step in Steps)
                {
                    if (step.DelayMs > 0) Thread.Sleep(step.DelayMs);
                    
                    if (step.IsKeyDown)
                        keybd_event(step.KeyCode, 0, 0, 0);
                    else
                        keybd_event(step.KeyCode, 0, 2, 0);
                }
            });
        }

        public void ExecuteAnalog(bool direction) { }
    }

    public class MacroStep
    {
        public byte KeyCode { get; set; }
        public bool IsKeyDown { get; set; }
        public int DelayMs { get; set; }
    }
}
