using NAudio.CoreAudioApi;

namespace KOYA_APP
{
    public class MuteSpeakerAction : IStreamDeckAction
    {
        public string Name => "Mute Glosniki";
        public string Description => "Wycisza/odcisza domyslne urzadzenie wyjsciowe";

        public void Execute()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
            }
            catch { /* Ignorujemy bledy audio */ }
        }
    }
}
