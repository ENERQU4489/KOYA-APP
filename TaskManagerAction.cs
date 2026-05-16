using System.Diagnostics;

namespace KOYA_APP
{
    public class TaskManagerAction : IStreamDeckAction
    {
        public string Name => "Menedzer Zadan";
        public string Icon => "\uE136";
        public string Category => "System & Okna";
        public string Description => "Otwiera taskmgr";

        public void Execute()
        {
            try { Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true }); }
            catch { }
        }

        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }
    }
}
