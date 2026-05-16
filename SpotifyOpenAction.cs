using System.Diagnostics;
using System.Linq;

namespace KOYA_APP
{
    public class SpotifyOpenAction : IStreamDeckAction
    {
        public string Name => "Spotify: Otwórz";
        public string Description => "Pokaż okno Spotify";
        public string Icon => "\uE189"; // Music icon
        public string Category => "Multimedia & Audio";

        public void Execute()
        {
            var spotifyProcess = Process.GetProcessesByName("Spotify")
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle));

            if (spotifyProcess != null)
            {
                // Przywróć okno
                IntPtr handle = spotifyProcess.MainWindowHandle;
                ShowWindow(handle, 9); // SW_RESTORE
                SetForegroundWindow(handle);
            }
            else
            {
                // Uruchom jeśli nie działa
                Process.Start(new ProcessStartInfo("spotify") { UseShellExecute = true });
            }
        }

        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(System.IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
    }
}
