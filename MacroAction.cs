using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KOYA_APP
{
    public class MacroAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Makro (Zaawansowane)";
        public string Icon => "\uE7C8";
        public string Category => "Automatyzacja & Narzędzia";
        public string Description => $"Sekwencja {Steps.Count} zdarzeń (KBD/Mysz)";
        
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x40;

        public void Execute()
        {
            if (Steps.Count == 0) return;
            
            Task.Run(() =>
            {
                foreach (var step in Steps)
                {
                    if (step.DelayMs > 0) Thread.Sleep(step.DelayMs);
                    
                    switch (step.Type)
                    {
                        case MacroStepType.Keyboard:
                            keybd_event(step.KeyCode, 0, step.IsDown ? 0u : 2u, 0);
                            break;
                        
                        case MacroStepType.MouseMove:
                            SetCursorPos(step.X, step.Y);
                            break;
                        
                        case MacroStepType.MouseButton:
                            uint flags = GetMouseFlags(step.Button, step.IsDown);
                            mouse_event(flags, 0, 0, 0, 0);
                            break;
                    }
                }
            });
        }

        private uint GetMouseFlags(int button, bool isDown)
        {
            if (button == 1) return isDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
            if (button == 2) return isDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
            if (button == 3) return isDown ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
            return 0;
        }

        public void ExecuteAnalog(bool direction) { }
    }

    public enum MacroStepType { Keyboard, MouseMove, MouseButton }

    public class MacroStep
    {
        public MacroStepType Type { get; set; }
        public byte KeyCode { get; set; }
        public bool IsDown { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Button { get; set; } // 1: Left, 2: Right, 3: Middle
        public int DelayMs { get; set; }
    }
}
