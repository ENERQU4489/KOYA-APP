using NAudio.CoreAudioApi;
using System.Diagnostics;

namespace KOYA_APP
{
    public class AppVolumeAction : IStreamDeckAction
    {
        public string Name { get; set; } = "App Volume";
        public string Icon => "\uE767";
        public string Description { get; set; } = "Control volume for a specific app";
        
        public int ProcessId { get; set; }
        public string AppName { get; set; } = "Unknown";

        public void Execute() 
        {
            var session = GetSession();
            if (session != null)
            {
                session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;
            }
        }

        public void ExecuteAnalog(bool direction)
        {
            var session = GetSession();
            if (session != null)
            {
                float current = session.SimpleAudioVolume.Volume;
                float step = 0.02f;
                float next = direction ? current + step : current - step;
                session.SimpleAudioVolume.Volume = Math.Clamp(next, 0f, 1f);
            }
        }

        private AudioSessionControl? GetSession()
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessionManager = device.AudioSessionManager;
            sessionManager.RefreshSessions();

            for (int i = 0; i < sessionManager.Sessions.Count; i++)
            {
                var session = sessionManager.Sessions[i];
                if (session.GetProcessID == (uint)ProcessId)
                {
                    return session;
                }
            }
            return null;
        }
    }
}
