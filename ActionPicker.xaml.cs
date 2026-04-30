using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace KOYA_APP
{
    public partial class ActionPicker : Window
    {
        public IStreamDeckAction? SelectedAction { get; set; }
        private List<byte> _capturedKeys = new List<byte>();

        public ActionPicker(bool isAnalog = false)
        {
            InitializeComponent();

            var allActions = new List<IStreamDeckAction>
            {
                new PlayPauseAction(),
                new NextTrackAction(),
                new PreviousTrackAction(),
                new VolumeAction(),
                new CopyAction(),
                new PasteAction(),
                new ScreenshotAction(),
                new TaskManagerAction(),
                new CloseWindowAction(),
                new FullscreenAction(),
                new AltTabAction(),
                new CustomShortcutAction(),
                new OpenAppAction(),
                new MuteMicrophoneAction(),
                new MuteSpeakerAction(),
                new SelectMicAction(),
                new WebZoomAction(),
                new AppVolumeAction()
            };

            if (isAnalog)
            {
                ActionsListBox.ItemsSource = allActions.Where(a => 
                    a is VolumeAction || 
                    a is MuteMicrophoneAction || 
                    a is WebZoomAction || 
                    a is AppVolumeAction).ToList();
            }
            else
            {
                ActionsListBox.ItemsSource = allActions;
            }
        }

        private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            ExtraSettingsPanel.Visibility = Visibility.Collapsed;
            DevicesComboBox.Visibility = Visibility.Collapsed;
            ShortcutTextBox.Visibility = Visibility.Collapsed;

            if (selected is SelectMicAction || selected is MuteMicrophoneAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DevicesComboBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz urzadzenie (Mikrofon):";
                
                try
                {
                    var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
                    DevicesComboBox.DisplayMemberPath = "FriendlyName";
                    DevicesComboBox.ItemsSource = devices;
                    if (devices.Count > 0) 
                    {
                        DevicesComboBox.SelectedIndex = 0;
                        DevicesComboBox.IsEnabled = true;
                    }
                    else
                    {
                        ExtraSettingsTitle.Text = "BRAK URZADZEN AUDIO!";
                        DevicesComboBox.IsEnabled = false;
                    }
                }
                catch (System.Exception ex)
                {
                    ExtraSettingsTitle.Text = "BLAD AUDIO: " + ex.Message;
                    DevicesComboBox.IsEnabled = false;
                }
            }
            else if (selected is AppVolumeAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DevicesComboBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz aplikacje:";

                try
                {
                    var enumerator = new MMDeviceEnumerator();
                    var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    var sessionManager = device.AudioSessionManager;
                    sessionManager.RefreshSessions();

                    var apps = new List<dynamic>();
                    for (int i = 0; i < sessionManager.Sessions.Count; i++)
                    {
                        var session = sessionManager.Sessions[i];
                        uint pid = session.GetProcessID;
                        if (pid == 0) continue;

                        string name;
                        try { name = Process.GetProcessById((int)pid).ProcessName; }
                        catch { name = session.DisplayName; }

                        if (string.IsNullOrEmpty(name)) name = "Unknown App";
                        apps.Add(new { Name = name, Id = (int)pid });
                    }

                    DevicesComboBox.DisplayMemberPath = "Name";
                    DevicesComboBox.ItemsSource = apps;
                    if (apps.Count > 0)
                    {
                        DevicesComboBox.SelectedIndex = 0;
                        DevicesComboBox.IsEnabled = true;
                    }
                }
                catch (System.Exception ex)
                {
                    ExtraSettingsTitle.Text = "BLAD MIXERA: " + ex.Message;
                }
            }
            else if (selected is CustomShortcutAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ShortcutTextBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wcisnij kombinacje klawiszy:";
                _capturedKeys.Clear();
                ShortcutTextBox.Text = "Kliknij tutaj i wcisnij klawisze...";
            }
        }

        private void ShortcutTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            int vk = KeyInterop.VirtualKeyFromKey(e.Key);
            if (vk == 0) return;
            byte vkByte = (byte)vk;
            if (!_capturedKeys.Contains(vkByte)) _capturedKeys.Add(vkByte);
            List<string> keyNames = _capturedKeys.Select(k => ((Key)KeyInterop.KeyFromVirtualKey(k)).ToString()).ToList();
            ShortcutTextBox.Text = string.Join(" + ", keyNames);
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            if (selected == null) return;

            if (selected is OpenAppAction appAction)
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Aplikacje (*.exe)|*.exe" };
                if (ofd.ShowDialog() == true) { appAction.Path = ofd.FileName; SelectedAction = appAction; }
            }
            else if (selected is CustomShortcutAction shortcutAction)
            {
                if (_capturedKeys.Count > 0) { shortcutAction.KeyCodes = new List<byte>(_capturedKeys); shortcutAction.KeysDisplay = ShortcutTextBox.Text; SelectedAction = shortcutAction; }
            }
            else if (selected is SelectMicAction micAction)
            {
                var device = DevicesComboBox.SelectedItem as MMDevice;
                if (device != null) { micAction.DeviceID = device.ID; micAction.DeviceName = device.FriendlyName; SelectedAction = micAction; }
            }
            else if (selected is MuteMicrophoneAction muteMicAction)
            {
                var device = DevicesComboBox.SelectedItem as MMDevice;
                if (device != null) { muteMicAction.DeviceId = device.ID; SelectedAction = muteMicAction; }
            }
            else if (selected is AppVolumeAction appVolAction)
            {
                var app = DevicesComboBox.SelectedItem as dynamic;
                if (app != null)
                {
                    appVolAction.ProcessId = app.Id;
                    appVolAction.AppName = app.Name;
                    appVolAction.Name = $"VOL: {app.Name}";
                    SelectedAction = appVolAction;
                }
            }
            else { SelectedAction = selected; }

            if (SelectedAction != null) { this.DialogResult = true; this.Close(); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
