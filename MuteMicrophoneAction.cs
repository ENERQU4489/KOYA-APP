using NAudio.CoreAudioApi;

namespace KOYA_APP
{
    public class MuteMicrophoneAction : IStreamDeckAction
    {
        public string Name => "Wycisz Mikrofon";
        public string Icon => "\uE1D6";
        public string Description => "Przelacz mikrofon (Wl/Wyl)";

        public string? DeviceId { get; set; }

        public void Execute() 
        { 
            try 
            { 
                var en = new MMDeviceEnumerator(); 
                var d = string.IsNullOrEmpty(DeviceId) 
                    ? en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications) 
                    : en.GetDevice(DeviceId);
                d.AudioEndpointVolume.Mute = !d.AudioEndpointVolume.Mute; 
            } catch {} 
        }

        public void ExecuteAnalog(bool direction) 
        { 
            try 
            { 
                var en = new MMDeviceEnumerator(); 
                var d = string.IsNullOrEmpty(DeviceId) 
                    ? en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications) 
                    : en.GetDevice(DeviceId);
                float step = 0.05f; 
                float cur = d.AudioEndpointVolume.MasterVolumeLevelScalar; 
                d.AudioEndpointVolume.MasterVolumeLevelScalar = direction ? System.Math.Min(1f, cur + step) : System.Math.Max(0f, cur - step); 
            } catch {} 
        }
    }
}
