using System.Diagnostics;

namespace KOYA_APP
{
    public class OpenAppAction : IStreamDeckAction
    {
        public string Name => "Otworz Aplikacje";
        public string Icon => "\uE1A5";
        public string Category => "Internet & Aplikacje";
        public string Description => $"Odpala: {Path}";
        public string? Path { get; set; }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(Path))
            {
                try { Process.Start(new ProcessStartInfo(Path) { UseShellExecute = true }); }
                catch { }
            }
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
