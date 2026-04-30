using System.IO;
using System.Text.Json;

namespace KOYA_APP
{
    public static class ConfigurationManager
    {
        private static readonly string ConfigPath = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static void SaveConfig(IStreamDeckAction?[] actions)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(actions, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }

        public static IStreamDeckAction?[] LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return new IStreamDeckAction[14];

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var actions = JsonSerializer.Deserialize<IStreamDeckAction?[]>(json);
                return actions ?? new IStreamDeckAction[14];
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
                return new IStreamDeckAction[14];
            }
        }
    }
}
