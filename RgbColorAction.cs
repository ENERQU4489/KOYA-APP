namespace KOYA_APP
{
    public class RgbColorAction : IStreamDeckAction
    {
        public string Name => "RGB Color";
        public string Description => Value > 0 ? $"Ustaw kolor: {Value}" : "Zmiana koloru podświetlenia (HUE)";
        public string Icon => "\uE790";
        public string Category => "Zarządzanie PC";

        public int Value { get; set; } = 180;

        public void Execute() 
        {
            // Logika ustawienia na sztywno
        }

        public void ExecuteAnalog(bool direction)
        {
            if (direction) Value = (Value + 10) % 360;
            else Value = (Value - 10 + 360) % 360;
        }

        public void ExecuteAbsolute(int value)
        {
            Value = (int)(value * 360.0 / 255.0);
            // Logika ustawienia na sztywno
        }
    }
}
