using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System;
using SharpHook;

namespace KOYA_APP
{
    public partial class ActionPicker : Window
{
    public IStreamDeckAction? SelectedAction { get; set; }
    private List<byte> _capturedKeys = new List<byte>();

    private List<MacroStep> _macroSteps = new List<MacroStep>();
    private bool _isRecordingMacro = false;
    private DateTime _lastMacroEventTime;
    private TaskPoolGlobalHook? _globalHook;
    private int _targetButtonIndex;
    private HidBackend _hid;

    public ActionPicker(int buttonIndex, HidBackend hid, bool isAnalog = false)
    {
        InitializeComponent();
        _targetButtonIndex = buttonIndex;
        _hid = hid;
        this.Closed += (s, e) => StopRecording();

        // Słuchaj hardware'u do wyzwalania nagrywania
        _hid.ButtonPressed += HandleHardwareButtonForMacro;

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
                new MacroAction(),
                new SoundboardAction(),
                new AIAssistantAction()
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
            PasteTextPanel.Visibility = Visibility.Collapsed;
            BrowseFileButton.Visibility = Visibility.Collapsed;
            MacroPanel.Visibility = Visibility.Collapsed;

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
            else if (selected is OpenAppAction appAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz aplikację (.exe) lub wpisz ścieżkę:";
                PasteTextInput.Text = appAction.Path;
            }
            else if (selected is OpenLinkAction ol)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz adres URL (np. https://google.com):";
                PasteTextInput.Text = ol.Url;
            }
            else if (selected is CreateFolderAction cf)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz ścieżkę folderu lub wybierz ikonę folderu:";
                PasteTextInput.Text = cf.FolderPath;
            }
            else if (selected is PowerShellAction ps)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wpisz komende lub sciezke do skryptu (.ps1):";
                PasteTextInput.Text = ps.ScriptContent;
            }
            else if (selected is MacroAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                MacroPanel.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Nagraj sekwencję (Klawiatura + Mysz):";
                _macroSteps.Clear();
                MacroStatusText.Text = "GOTOWY DO NAGRYWANIA";
            }
            else if (selected is SoundboardAction sb)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz plik dźwiękowy (MP3/WAV) lub wpisz ścieżkę:";
                PasteTextInput.Text = sb.FilePath;
            }
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            if (selected is SoundboardAction)
            {
                var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Audio Files (*.mp3, *.wav)|*.mp3;*.wav" };
                if (ofd.ShowDialog() == true) PasteTextInput.Text = ofd.FileName;
            }
            else if (selected is OpenAppAction || selected is PowerShellAction)
            {
                var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Executable Files (*.exe, *.ps1, *.bat)|*.exe;*.ps1;*.bat|All Files (*.*)|*.*" };
                if (ofd.ShowDialog() == true) PasteTextInput.Text = ofd.FileName;
            }
            else if (selected is CreateFolderAction)
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        PasteTextInput.Text = fbd.SelectedPath;
                }
            }
        }

        private void HandleHardwareButtonForMacro(int index)
        {
            if (index == _targetButtonIndex && ActionsListBox.SelectedItem is MacroAction)
            {
                Dispatcher.Invoke(() => RecordMacro_Click(this, new RoutedEventArgs()));
            }
        }

        private void RecordMacro_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecordingMacro)
            {
                StartGlobalRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void StartGlobalRecording()
        {
            _isRecordingMacro = true;
            _macroSteps.Clear();
            _lastMacroEventTime = DateTime.Now;
            RecordMacroButtonText.Text = "STOP (LUB NACIŚNIJ PRZYCISK)";
            RecordMacroButton.Visibility = Visibility.Visible;
            MacroStatusText.Text = "NAGRYWANIE W TOKU...";
            MacroSummaryList.Items.Clear();

            _globalHook = new TaskPoolGlobalHook();
            
            _globalHook.KeyPressed += (s, e) => AddGlobalKeyStep(e, true);
            _globalHook.KeyReleased += (s, e) => AddGlobalKeyStep(e, false);
            
            _globalHook.MouseMoved += (s, e) => AddGlobalMouseStep(e, MacroStepType.MouseMove, false);
            _globalHook.MousePressed += (s, e) => AddGlobalMouseStep(e, MacroStepType.MouseButton, true);
            _globalHook.MouseReleased += (s, e) => AddGlobalMouseStep(e, MacroStepType.MouseButton, false);

            _globalHook.RunAsync();
        }

        private void AddGlobalMouseStep(MouseHookEventArgs e, MacroStepType type, bool isDown)
        {
            if (!_isRecordingMacro) return;
            
            var now = DateTime.Now;
            int delay = (int)(now - _lastMacroEventTime).TotalMilliseconds;
            _lastMacroEventTime = now;

            var step = new MacroStep
            {
                Type = type,
                X = e.Data.X,
                Y = e.Data.Y,
                Button = (int)e.Data.Button,
                IsDown = isDown,
                DelayMs = delay
            };

            lock (_macroSteps) _macroSteps.Add(step);
            UpdateSummaryUI(step);
        }

        private void AddGlobalKeyStep(KeyboardHookEventArgs e, bool isDown)
        {
            if (!_isRecordingMacro) return;

            var now = DateTime.Now;
            int delay = (int)(now - _lastMacroEventTime).TotalMilliseconds;
            _lastMacroEventTime = now;

            byte vk = (byte)e.Data.RawCode; 

            var step = new MacroStep
            {
                Type = MacroStepType.Keyboard,
                KeyCode = vk,
                IsDown = isDown,
                DelayMs = delay
            };

            lock (_macroSteps) _macroSteps.Add(step);
            UpdateSummaryUI(step);
        }

        private int _moveCount = 0;

        private void UpdateSummaryUI(MacroStep step)
        {
            Dispatcher.Invoke(() => {
                MacroStatusText.Text = $"NAGRANO: {_macroSteps.Count} ZDARZEŃ";
                
                if (step.Type == MacroStepType.MouseMove)
                {
                    _moveCount++;
                    if (_moveCount % 10 == 0) 
                    {
                        var lastItem = MacroSummaryList.Items.Cast<dynamic>().LastOrDefault();
                        if (lastItem != null && (MacroStepType)lastItem.Type == MacroStepType.MouseMove)
                        {
                            var newItem = new {
                                Type = MacroStepType.MouseMove,
                                Icon = "\uE961",
                                Description = $"Ruch myszy -> {step.X}, {step.Y} ({_moveCount} pkt)"
                            };
                            int index = MacroSummaryList.Items.Count - 1;
                            MacroSummaryList.Items.RemoveAt(index);
                            MacroSummaryList.Items.Add(newItem);
                            return;
                        }
                    }
                    else if (_moveCount > 1) return;
                }
                else
                {
                    _moveCount = 0;
                }

                MacroSummaryList.Items.Add(new {
                    Type = step.Type,
                    Icon = GetStepIcon(step),
                    Description = GetStepDescription(step)
                });

                if (MacroSummaryList.Items.Count > 50) MacroSummaryList.Items.RemoveAt(0);
            });
        }

        private string GetStepIcon(MacroStep step) => step.Type switch {
            MacroStepType.Keyboard => "\uE765",
            MacroStepType.MouseButton => "\uE962",
            MacroStepType.MouseMove => "\uE961",
            _ => "\uE10C"
        };

        private string GetStepDescription(MacroStep step) => step.Type switch {
            MacroStepType.Keyboard => $"{(step.IsDown ? "Wciśnięto" : "Zwolniono")} klawisz (0x{step.KeyCode:X})",
            MacroStepType.MouseButton => $"{(step.IsDown ? "Kliknięto" : "Zwolniono")} przycisk myszy {step.Button}",
            MacroStepType.MouseMove => $"Ruch myszy -> {step.X}, {step.Y}",
            _ => "Nieznana akcja"
        };

        private void StopRecording()
        {
            if (!_isRecordingMacro) return;
            _isRecordingMacro = false;
            
            _globalHook?.Dispose();
            _globalHook = null;

            Dispatcher.Invoke(() => {
                RecordMacroButtonText.Text = "NAGRAJ";
                RecordMacroButton.Visibility = Visibility.Collapsed;
                MacroStatusText.Text = $"ZAPISANO {_macroSteps.Count} KROKÓW";
            });
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
                appAction.Path = PasteTextInput.Text;
                SelectedAction = appAction;
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
                cfAction.FolderPath = PasteTextInput.Text;
                SelectedAction = cfAction;
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
                bool adding = true;
                while (adding)
                {
                    ActionPicker subPicker = new ActionPicker(_targetButtonIndex, _hid, false) { Owner = this };
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
            else if (selected is SoundboardAction sbAction)
            {
                sbAction.FilePath = PasteTextInput.Text;
                SelectedAction = sbAction;
            }
            else { SelectedAction = selected; }

            if (SelectedAction != null) { this.DialogResult = true; this.Close(); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
