using NAudio.CoreAudioApi;

namespace KOYA_APP
{
    public class SelectMicAction : IStreamDeckAction
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Name => string.IsNullOrEmpty(DeviceName) ? "Wybierz Mikrofon" : $"Mic: {DeviceName}";
        public string Description => "Przełącza wyciszenie wybranego urządzenia";

        public void Execute()
        {
            if (string.IsNullOrEmpty(DeviceID)) return;
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDevice(DeviceID);
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
            }
            catch { }
        }
    }
}