using NAudio.CoreAudioApi;

namespace KOYA_APP
{
    public class MuteSpeakerAction : IStreamDeckAction
    {
        public string Name => "Mute Glosniki";
        public string Icon => "\uE74F";
        public string Category => "Multimedia & Audio";
        public string Description => "Wycisz/Odcisz glosniki";

        public void Execute()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
            }
            catch { }
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
