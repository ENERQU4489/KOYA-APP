using System;
using NAudio.Wave;
using System.Threading.Tasks;
using System.Linq;

namespace KOYA_APP
{
    public class VirtualMicAction : IStreamDeckAction
    {
        public string Name => "Virtual Mic Link";
        public string Icon => "\uE720"; // Mic icon
        public string Category => "Multimedia & Audio";
        public string Description => IsRunning ? $"Miksowanie: {InputDeviceName} -> {OutputDeviceName}" : "Przesyłaj mikrofon do Virtual Cable";

        public string? InputDeviceId { get; set; }
        public string? InputDeviceName { get; set; }
        public string? OutputDeviceId { get; set; }
        public string? OutputDeviceName { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsRunning { get; private set; }

        private WaveInEvent? _waveIn;
        private WaveOutEvent? _waveOut;
        private BufferedWaveProvider? _buffer;

        public void Execute()
        {
            if (IsRunning)
            {
                StopLink();
            }
            else
            {
                StartLink();
            }
        }

        private void StartLink()
        {
            try
            {
                int inIdx = GetInputIndex();
                int outIdx = GetOutputIndex();

                if (inIdx == -1 || outIdx == -1) return;

                _waveIn = new WaveInEvent { DeviceNumber = inIdx, WaveFormat = new WaveFormat(44100, 1) };
                _buffer = new BufferedWaveProvider(_waveIn.WaveFormat) { DiscardOnBufferOverflow = true };
                
                _waveOut = new WaveOutEvent { DeviceNumber = outIdx };
                _waveOut.Init(_buffer);

                _waveIn.DataAvailable += (s, e) => {
                    _buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                };

                _waveIn.StartRecording();
                _waveOut.Play();
                IsRunning = true;
            }
            catch { StopLink(); }
        }

        private void StopLink()
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveIn = null;
            _waveOut = null;
            _buffer = null;
            IsRunning = false;
        }

        private int GetInputIndex()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                if (WaveIn.GetCapabilities(i).ProductName == InputDeviceName) return i;
            }
            return -1;
        }

        private int GetOutputIndex()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName == OutputDeviceName) return i;
            }
            return -1;
        }

        public void ExecuteAnalog(bool direction) { }
        public void ExecuteAbsolute(int value) { }
    }
}
