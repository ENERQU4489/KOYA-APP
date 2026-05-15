using System.IO;
using System.Text.Json;

namespace KOYA_APP
{
    public class AppConfig
    {
        public bool IsFirstStart { get; set; } = true;
        public bool EnablePopups { get; set; } = true;
        public string? GeminiKey { get; set; }
        public string? GroqKey { get; set; }
        public string? OpenAIKey { get; set; }
        public IStreamDeckAction?[] Actions { get; set; } = new IStreamDeckAction?[14];
    }

    public static class ConfigurationManager
    {
        private static readonly string ConfigPath = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static void SaveConfig(IStreamDeckAction?[] actions, bool isFirstStart = false, string? geminiKey = null, bool? enablePopups = null, string? groqKey = null, string? openAIKey = null)
        {
            try
            {
                var current = LoadConfig();
                var config = new AppConfig 
                { 
                    Actions = actions, 
                    IsFirstStart = isFirstStart,
                    GeminiKey = geminiKey ?? current.GeminiKey,
                    GroqKey = groqKey ?? current.GroqKey,
                    OpenAIKey = openAIKey ?? current.OpenAIKey,
                    EnablePopups = enablePopups ?? current.EnablePopups
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }

        public static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return new AppConfig();

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                return config ?? new AppConfig();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
                return new AppConfig();
            }
        }
    }
}
