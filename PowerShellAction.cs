using System;
using System.Diagnostics;

namespace KOYA_APP
{
    public class PowerShellAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Skrypt PowerShell";
        public string Icon => "\uE756"; // Ikona terminala/kodu
        public string Category => "Automatyzacja & Narzędzia";
        public string Description { get; set; } = "Uruchamia skrypt lub komendę PowerShell";
        
        public string ScriptContent { get; set; } = "";
        public bool RunAsAdmin { get; set; } = false;

        public void Execute()
        {
            if (string.IsNullOrEmpty(ScriptContent)) return;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{ScriptContent}\"",
                    UseShellExecute = RunAsAdmin,
                    CreateNoWindow = !RunAsAdmin,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                if (RunAsAdmin)
                {
                    psi.Verb = "runas";
                }

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PowerShell Error: {ex.Message}");
            }
        }

        public void ExecuteAnalog(bool direction) { /* Nie dotyczy przycisków */ }
        public void ExecuteAbsolute(int value) { }
    }
}
