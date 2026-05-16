using System.Text.Json.Serialization;

namespace KOYA_APP
{
    [JsonDerivedType(typeof(PlayPauseAction), typeDiscriminator: "PlayPause")]
    [JsonDerivedType(typeof(NextTrackAction), typeDiscriminator: "NextTrack")]
    [JsonDerivedType(typeof(PreviousTrackAction), typeDiscriminator: "PreviousTrack")]
    [JsonDerivedType(typeof(VolumeAction), typeDiscriminator: "Volume")]
    [JsonDerivedType(typeof(CopyAction), typeDiscriminator: "Copy")]
    [JsonDerivedType(typeof(PasteAction), typeDiscriminator: "Paste")]
    [JsonDerivedType(typeof(PasteTextAction), typeDiscriminator: "PasteText")]
    [JsonDerivedType(typeof(BrightnessAction), typeDiscriminator: "Brightness")]
    [JsonDerivedType(typeof(PowerShellAction), typeDiscriminator: "PowerShell")]
    [JsonDerivedType(typeof(ScreenshotAction), typeDiscriminator: "Screenshot")]
    [JsonDerivedType(typeof(TaskManagerAction), typeDiscriminator: "TaskManager")]
    [JsonDerivedType(typeof(CloseWindowAction), typeDiscriminator: "CloseWindow")]
    [JsonDerivedType(typeof(FullscreenAction), typeDiscriminator: "Fullscreen")]
    [JsonDerivedType(typeof(AltTabAction), typeDiscriminator: "AltTab")]
    [JsonDerivedType(typeof(CustomShortcutAction), typeDiscriminator: "Shortcut")]
    [JsonDerivedType(typeof(OpenAppAction), typeDiscriminator: "OpenApp")]
    [JsonDerivedType(typeof(OpenLinkAction), typeDiscriminator: "OpenLink")]
    [JsonDerivedType(typeof(BrowserBackAction), typeDiscriminator: "BrowserBack")]
    [JsonDerivedType(typeof(BrowserForwardAction), typeDiscriminator: "BrowserForward")]
    [JsonDerivedType(typeof(BrowserRefreshAction), typeDiscriminator: "BrowserRefresh")]
    [JsonDerivedType(typeof(CreateFolderAction), typeDiscriminator: "CreateFolder")]
    [JsonDerivedType(typeof(MultiAction), typeDiscriminator: "MultiAction")]
    [JsonDerivedType(typeof(ShutdownAction), typeDiscriminator: "Shutdown")]
    [JsonDerivedType(typeof(MuteMicrophoneAction), typeDiscriminator: "MuteMic")]
    [JsonDerivedType(typeof(MuteSpeakerAction), typeDiscriminator: "MuteSpeaker")]
    [JsonDerivedType(typeof(SelectMicAction), typeDiscriminator: "SelectMic")]
    [JsonDerivedType(typeof(WebZoomAction), typeDiscriminator: "WebZoom")]
    [JsonDerivedType(typeof(AppVolumeAction), typeDiscriminator: "AppVolume")]
    [JsonDerivedType(typeof(SpotifyLikeAction), typeDiscriminator: "SpotifyLike")]
    [JsonDerivedType(typeof(SpotifyOpenAction), typeDiscriminator: "SpotifyOpen")]
    [JsonDerivedType(typeof(MacroAction), typeDiscriminator: "Macro")]
    [JsonDerivedType(typeof(SoundboardAction), typeDiscriminator: "Soundboard")]
    [JsonDerivedType(typeof(AIAssistantAction), typeDiscriminator: "AIAssistant")]
    [JsonDerivedType(typeof(FanSpeedAction), typeDiscriminator: "FanSpeed")]
    [JsonDerivedType(typeof(RgbColorAction), typeDiscriminator: "RgbColor")]
    [JsonDerivedType(typeof(RgbLightAction), typeDiscriminator: "RgbLight")]
    public interface IStreamDeckAction
    {
        string Name { get; }
        string Description { get; }
        string Icon { get; }
        string Category { get; }
        void Execute(); // Dla klikniecia
        void ExecuteAnalog(bool direction); // true = w prawo / glosniej, false = w lewo / ciszej
    }
}

