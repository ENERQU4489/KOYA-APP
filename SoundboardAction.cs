using System;
using NAudio.Wave;
using System.Threading.Tasks;

namespace KOYA_APP
{
    public class SoundboardAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Soundboard";
        public string Description { get; set; } = "Odtwórz dźwięk";
        public string Icon => "\uE7F6"; // Volume icon
        public string FilePath { get; set; } = "";

        public void Execute()
        {
            if (string.IsNullOrEmpty(FilePath) || !System.IO.File.Exists(FilePath)) return;

            Task.Run(() =>
            {
                try
                {
                    using (var audioFile = new AudioFileReader(FilePath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Soundboard Error: {ex.Message}");
                }
            });
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
