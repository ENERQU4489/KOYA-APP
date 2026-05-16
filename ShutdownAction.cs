using System.Diagnostics;

namespace KOYA_APP
{
    public class ShutdownAction : IStreamDeckAction
    {
        public string Name => "Wyłącz PC";
        public string Icon => "\uE7E8"; // Ikona Power / Shutdown
        public string Category => "System & Okna";
        public string Description => "Natychmiast wyłącza komputer";

        public void Execute()
        {
            try
            {
                // Wyłączenie komputera: shutdown /s /t 0
                Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") 
                { 
                    CreateNoWindow = true, 
                    UseShellExecute = false 
                });
            }
            catch { }
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
