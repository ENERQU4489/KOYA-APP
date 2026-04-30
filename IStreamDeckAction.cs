using System.Text.Json.Serialization;

namespace KOYA_APP
{
    [JsonDerivedType(typeof(PlayPauseAction), typeDiscriminator: "PlayPause")]
    [JsonDerivedType(typeof(NextTrackAction), typeDiscriminator: "NextTrack")]
    [JsonDerivedType(typeof(PreviousTrackAction), typeDiscriminator: "PreviousTrack")]
    [JsonDerivedType(typeof(VolumeAction), typeDiscriminator: "Volume")]
    [JsonDerivedType(typeof(CopyAction), typeDiscriminator: "Copy")]
    [JsonDerivedType(typeof(PasteAction), typeDiscriminator: "Paste")]
    [JsonDerivedType(typeof(ScreenshotAction), typeDiscriminator: "Screenshot")]
    [JsonDerivedType(typeof(TaskManagerAction), typeDiscriminator: "TaskManager")]
    [JsonDerivedType(typeof(CloseWindowAction), typeDiscriminator: "CloseWindow")]
    [JsonDerivedType(typeof(FullscreenAction), typeDiscriminator: "Fullscreen")]
    [JsonDerivedType(typeof(AltTabAction), typeDiscriminator: "AltTab")]
    [JsonDerivedType(typeof(CustomShortcutAction), typeDiscriminator: "Shortcut")]
    [JsonDerivedType(typeof(OpenAppAction), typeDiscriminator: "OpenApp")]
    [JsonDerivedType(typeof(MuteMicrophoneAction), typeDiscriminator: "MuteMic")]
    [JsonDerivedType(typeof(MuteSpeakerAction), typeDiscriminator: "MuteSpeaker")]
    [JsonDerivedType(typeof(SelectMicAction), typeDiscriminator: "SelectMic")]
    [JsonDerivedType(typeof(WebZoomAction), typeDiscriminator: "WebZoom")]
    [JsonDerivedType(typeof(AppVolumeAction), typeDiscriminator: "AppVolume")]
    public interface IStreamDeckAction
    {
        string Name { get; }
        string Description { get; }
        string Icon { get; }
        void Execute(); // Dla klikniecia
        void ExecuteAnalog(bool direction); // true = w prawo / glosniej, false = w lewo / ciszej
    }
}

