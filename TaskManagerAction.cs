using System.Diagnostics;

namespace KOYA_APP
{
    public class TaskManagerAction : IStreamDeckAction
    {
        public string Name => "Menedzer Zadan";
        public string Description => "Otwiera systemowy Menedzer Zadan";

        public void Execute()
        {
            try { Process.Start("taskmgr.exe"); }
            catch { /* Ignorujemy bledy */ }
        }
    }
}
