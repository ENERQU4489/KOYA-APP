using System.Diagnostics;

namespace KOYA_APP
{
    public class OpenLinkAction : IStreamDeckAction
    {
        public string Name => "Otwórz Link";
        public string Icon => "\uE71B"; // Ikona kuli ziemskiej / linku
        public string Description => $"Otwiera: {Url}";
        public string? Url { get; set; }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(Url))
            {
                try 
                { 
                    Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true }); 
                }
                catch { }
            }
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
