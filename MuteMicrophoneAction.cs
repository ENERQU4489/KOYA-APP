using NAudio.CoreAudioApi;
using System.Data;

namespace KOYA_APP
{
    public class MuteMicrophoneAction : IStreamDeckAction
    {
        public string Name => "Wycisz Mikrofon";
        public string Description => "Włącza/Wyłącza domyślny mikrofon";

        public void Execute()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
            }
            catch { }
        }
    }
}