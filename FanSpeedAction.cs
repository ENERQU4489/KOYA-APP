namespace KOYA_APP
{
    public class FanSpeedAction : IStreamDeckAction
    {
        public string Name => "Fan Speed";
        public string Description => Value > 0 ? $"Ustaw prędkość: {Value}%" : "Regulacja prędkości wentylatorów";
        public string Icon => "\uE9F5";
        public string Category => "Zarządzanie PC";

        public int Value { get; set; } = 50;

        public void Execute() 
        {
            // Logika ustawienia na sztywno (Value)
        }

        public void ExecuteAnalog(bool direction)
        {
            if (direction) Value = Math.Min(100, Value + 5);
            else Value = Math.Max(0, Value - 5);
        }

        public void ExecuteAbsolute(int value)
        {
            Value = (int)(value * 100.0 / 255.0);
            // Logika ustawienia na sztywno (Value)
        }
    }
}
