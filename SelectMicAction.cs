using NAudio.CoreAudioApi;

namespace KOYA_APP
{
    public class SelectMicAction : IStreamDeckAction
    {
        public string Name => "Wybierz Mikrofon";
        public string Icon => "\uE1F6";
        public string Category => "Multimedia & Audio";
        public string Description => $"Ustawia: {DeviceName}";
        public string? DeviceID { get; set; }
        public string? DeviceName { get; set; }

        public void Execute() { }
        public void ExecuteAnalog(bool direction) { }
    }
}
