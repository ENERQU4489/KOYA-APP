using System.IO;
using System.Windows.Forms;

namespace KOYA_APP
{
    public class CreateFolderAction : IStreamDeckAction
    {
        public string Name => "Utwórz Folder";
        public string Icon => "\uE8B7"; // Ikona folderu
        public string Category => "Automatyzacja & Narzędzia";
        public string Description => $"Tworzy folder: {FolderPath}";
        public string? FolderPath { get; set; }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                }
                catch { }
            }
        }

        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }
    }
}
