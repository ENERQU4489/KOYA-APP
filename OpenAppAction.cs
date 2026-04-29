using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace KOYA_APP
{
    public class OpenAppAction : IStreamDeckAction
    {
        public string? Path { get; set; }
        public string Name => string.IsNullOrEmpty(Path) ? "Otwórz aplikacje" : $"Uruchom: {System.IO.Path.GetFileNameWithoutExtension(Path)}";
        public string Description => string.IsNullOrEmpty(Path) ? "Uruchamia wybrany program." : $"Sciezka: {Path}";

        public void Execute()
        {
            if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Path,
                        UseShellExecute = true,
                        // To ustawienie pomaga przy plikach exe wymagajacych admina:
                        Verb = "open",
                        WorkingDirectory = System.IO.Path.GetDirectoryName(Path)
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // Wyswietli blad zamiast wywalac caly program
                    MessageBox.Show($"Nie udalo sie uruchomic aplikacji: {ex.Message}", "Blad", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}