namespace KOYA_APP
{
    public class RgbLightAction : IStreamDeckAction
    {
        public string Name => "RGB Light";
        public string Description => Value > 0 ? $"Ustaw jasność: {Value}%" : "Regulacja natężenia światła RGB";
        public string Icon => "\uE706";
        public string Category => "Zarządzanie PC";

        public int Value { get; set; } = 80;

        public void Execute() 
        {
            // Logika ustawienia na sztywno
        }

        public void ExecuteAnalog(bool direction)
        {
            if (direction) Value = Math.Min(100, Value + 10);
            else Value = Math.Max(0, Value - 10);
        }
    }
}
