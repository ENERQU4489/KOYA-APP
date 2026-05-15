using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System;

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
                new BrightnessAction(),
                new CopyAction(),
                new PasteAction(),
                new ScreenshotAction(),
                new TaskManagerAction(),
                new CloseWindowAction(),
                new FullscreenAction(),
                new AltTabAction(),
                new CustomShortcutAction(),
                new PowerShellAction(),
                new PasteTextAction(),
                new OpenAppAction(),
                new OpenLinkAction(),
                new BrowserBackAction(),
                new BrowserForwardAction(),
                new BrowserRefreshAction(),
                new CreateFolderAction(),
                new MultiAction(),
                new ShutdownAction(),
                new MuteMicrophoneAction(),
                new MuteSpeakerAction(),
                new SelectMicAction(),
                new WebZoomAction(),
                new AppVolumeAction(),
                new SpotifyLikeAction(),
                new SpotifyOpenAction(),
                new MacroAction()
            };

            if (isAnalog)
            {
                ActionsListBox.ItemsSource = allActions.Where(a => 
                    a is VolumeAction || 
                    a is BrightnessAction ||
                    a is MuteMicrophoneAction || 
                    a is WebZoomAction || 
                    a is AppVolumeAction).ToList();
            }
            else
            {
                ActionsListBox.ItemsSource = allActions.Where(a => !(a is BrightnessAction)).ToList();
            }
        }

        private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            ExtraSettingsPanel.Visibility = Visibility.Collapsed;
            DevicesComboBox.Visibility = Visibility.Collapsed;
            ShortcutTextBox.Visibility = Visibility.Collapsed;
            PasteTextInput.Visibility = Visibility.Collapsed;

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
            else if (selected is BrightnessAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DevicesComboBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz ekran do sterowania:";

                try
                {
                    var monitors = new List<KeyValuePair<string, string>>();
                    using (var searcher = new System.Management.ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID"))
                    {
                        foreach (System.Management.ManagementObject obj in searcher.Get())
                        {
                            var nameBytes = obj["UserFriendlyName"] as ushort[];
                            var name = nameBytes != null ? new string(nameBytes.Select(b => (char)b).ToArray()).TrimEnd('\0') : "Monitor";
                            var instanceName = obj["InstanceName"]?.ToString() ?? "Unknown";
                            monitors.Add(new KeyValuePair<string, string>(instanceName, name));
                        }
                    }

                    if (monitors.Count == 0)
                    {
                        monitors.Add(new KeyValuePair<string, string>("", "Domyślny Monitor"));
                    }

                    DevicesComboBox.DisplayMemberPath = "Value";
                    DevicesComboBox.ItemsSource = monitors;
                    DevicesComboBox.SelectedIndex = 0;
                    DevicesComboBox.IsEnabled = true;
                }
                catch (System.Exception ex)
                {
                    ExtraSettingsTitle.Text = "BLAD WMI: " + ex.Message;
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
            else if (selected is PasteTextAction pt)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextInput.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz tekst do wklejenia:";
                PasteTextInput.Text = pt.TextToPaste;
            }
            else if (selected is OpenLinkAction ol)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextInput.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz adres URL (np. https://google.com):";
                PasteTextInput.Text = ol.Url;
            }
            else if (selected is CreateFolderAction cf)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextInput.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz ścieżkę folderu lub wybierz po kliknięciu Zastosuj:";
                PasteTextInput.Text = cf.FolderPath;
            }
            else if (selected is PowerShellAction ps)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextInput.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz komende lub sciezke do skryptu (.ps1):";
                PasteTextInput.Text = ps.ScriptContent;
            }
            else if (selected is MacroAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                MacroPanel.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Nagraj sekwencję klawiszy:";
                _macroSteps.Clear();
                MacroStatusText.Text = "GOTOWY DO NAGRYWANIA";
            }
        }

        private List<MacroStep> _macroSteps = new List<MacroStep>();
        private bool _isRecordingMacro = false;
        private DateTime _lastMacroEventTime;

        private void RecordMacro_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecordingMacro)
            {
                _isRecordingMacro = true;
                _macroSteps.Clear();
                _lastMacroEventTime = DateTime.Now;
                RecordMacroButtonText.Text = "STOP";
                MacroStatusText.Text = "NAGRYWANIE... (Wciskaj klawisze)";
                this.PreviewKeyDown += ActionPicker_PreviewKeyDown;
                this.PreviewKeyUp += ActionPicker_PreviewKeyUp;
            }
            else
            {
                StopRecording();
            }
        }

        private void StopRecording()
        {
            _isRecordingMacro = false;
            RecordMacroButtonText.Text = "NAGRAJ";
            MacroStatusText.Text = $"ZAPISANO {_macroSteps.Count} KROKÓW";
            this.PreviewKeyDown -= ActionPicker_PreviewKeyDown;
            this.PreviewKeyUp -= ActionPicker_PreviewKeyUp;
        }

        private void ActionPicker_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_isRecordingMacro) return;
            e.Handled = true;
            AddMacroStep(e.Key, true);
        }

        private void ActionPicker_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!_isRecordingMacro) return;
            e.Handled = true;
            AddMacroStep(e.Key, false);
        }

        private void AddMacroStep(Key key, bool isDown)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk == 0) return;

            var now = DateTime.Now;
            int delay = (int)(now - _lastMacroEventTime).TotalMilliseconds;
            _lastMacroEventTime = now;

            _macroSteps.Add(new MacroStep
            {
                KeyCode = (byte)vk,
                IsKeyDown = isDown,
                DelayMs = delay
            });

            MacroStatusText.Text = $"NAGRYWANIE: {_macroSteps.Count} KROKÓW...";
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
            else if (selected is BrightnessAction brightAction)
            {
                if (DevicesComboBox.SelectedItem is KeyValuePair<string, string> monitor)
                {
                    brightAction.MonitorId = monitor.Key;
                    brightAction.MonitorName = monitor.Value;
                    brightAction.Name = $"Jasność: {monitor.Value}";
                    SelectedAction = brightAction;
                }
            }
            else if (selected is PasteTextAction ptAction)
            {
                ptAction.TextToPaste = PasteTextInput.Text;
                SelectedAction = ptAction;
            }
            else if (selected is OpenLinkAction olAction)
            {
                olAction.Url = PasteTextInput.Text;
                SelectedAction = olAction;
            }
            else if (selected is CreateFolderAction cfAction)
            {
                if (string.IsNullOrEmpty(PasteTextInput.Text))
                {
                    using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            cfAction.FolderPath = fbd.SelectedPath;
                            SelectedAction = cfAction;
                        }
                    }
                }
                else
                {
                    cfAction.FolderPath = PasteTextInput.Text;
                    SelectedAction = cfAction;
                }
            }
            else if (selected is PowerShellAction psAction)
            {
                psAction.ScriptContent = PasteTextInput.Text;
                SelectedAction = psAction;
            }
            else if (selected is MacroAction macro)
            {
                if (_macroSteps.Count > 0)
                {
                    macro.Steps = new List<MacroStep>(_macroSteps);
                    SelectedAction = macro;
                }
            }
            else if (selected is MultiAction multi)
            {
                // Kaskadowe dodawanie akcji do MultiAction
                bool adding = true;
                while (adding)
                {
                    ActionPicker subPicker = new ActionPicker(false) { Owner = this };
                    // Ukrywamy MultiAction w pod-pickerze, żeby uniknąć nieskończonej rekurencji
                    var currentList = (subPicker.ActionsListBox.ItemsSource as List<IStreamDeckAction>);
                    if (currentList != null)
                    {
                        subPicker.ActionsListBox.ItemsSource = currentList.Where(a => !(a is MultiAction)).ToList();
                    }
                    
                    if (subPicker.ShowDialog() == true && subPicker.SelectedAction != null)
                    {
                        multi.Actions.Add(subPicker.SelectedAction);
                    }
                    else
                    {
                        adding = false;
                    }
                }
                
                if (multi.Actions.Count > 0)
                {
                    SelectedAction = multi;
                }
            }
            else { SelectedAction = selected; }

            if (SelectedAction != null) { this.DialogResult = true; this.Close(); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
