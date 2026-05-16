using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        this.Closed += (s, e) => {
            StopRecording();
            _hid.ButtonPressed -= HandleHardwareButtonForMacro;
        };

        _hid.ButtonPressed += HandleHardwareButtonForMacro;

        _allActions = new List<IStreamDeckAction>
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
                new AIAssistantAction(),
                new FanSpeedAction(),
                new RgbColorAction(),
                new RgbLightAction()
                };

                if (isAnalog)
                {
                _allActions = _allActions.Where(a => 
                    a is VolumeAction || 
                    a is BrightnessAction ||
                    a is MuteMicrophoneAction || 
                    a is WebZoomAction || 
                    a is AppVolumeAction ||
                    a is FanSpeedAction ||
                    a is RgbColorAction ||
                    a is RgbLightAction).ToList();
                }            else
            {
                _allActions = _allActions.Where(a => !(a is BrightnessAction)).ToList();
            }

            ApplyFilter();
        }

        private List<IStreamDeckAction> _allActions;

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allActions == null || ActionsListBox == null) return;

            string query = SearchBox.Text.ToLower();

            var filtered = _allActions.Where(a => {
                return string.IsNullOrEmpty(query) || 
                       a.Name.ToLower().Contains(query) || 
                       a.Description.ToLower().Contains(query) ||
                       a.Category.ToLower().Contains(query);
            }).ToList();

            var view = CollectionViewSource.GetDefaultView(filtered);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            ActionsListBox.ItemsSource = view;
        }

        private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            ExtraSettingsPanel.Visibility = Visibility.Collapsed;
            DeviceSettingsGroup.Visibility = Visibility.Collapsed;
            ShortcutSettingsGroup.Visibility = Visibility.Collapsed;
            PasteTextPanel.Visibility = Visibility.Collapsed;
            ValueSliderPanel.Visibility = Visibility.Collapsed;
            MacroPanel.Visibility = Visibility.Collapsed;

            if (selected == null) return;

            if (selected is SelectMicAction || selected is MuteMicrophoneAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DeviceSettingsGroup.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "URZĄDZENIA AUDIO";
                
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
                        DevicesComboBox.IsEnabled = false;
                    }
                }
                catch (System.Exception)
                {
                    DevicesComboBox.IsEnabled = false;
                }
            }
            else if (selected is AppVolumeAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DeviceSettingsGroup.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "MIKSER GŁOŚNOŚCI";
                
                try
                {
                    var enumerator = new MMDeviceEnumerator();
                    var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    var sessionManager = device.AudioSessionManager;
                    var apps = new List<dynamic>();
                    for (int i = 0; i < sessionManager.Sessions.Count; i++)
                    {
                        var session = sessionManager.Sessions[i];
                        uint pid = session.GetProcessID;
                        if (pid == 0) continue;
                        var process = System.Diagnostics.Process.GetProcessById((int)pid);
                        apps.Add(new { Id = (int)pid, Name = process.ProcessName });
                    }
                    DevicesComboBox.DisplayMemberPath = "Name";
                    DevicesComboBox.ItemsSource = apps;
                    if (apps.Count > 0) DevicesComboBox.SelectedIndex = 0;
                }
                catch (System.Exception) { }
            }
            else if (selected is BrightnessAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DeviceSettingsGroup.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "KONTROLA EKRANU";
                
                var monitors = new List<KeyValuePair<string, string>>();
                try {
                    using (var searcher = new System.Management.ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID"))
                    {
                        foreach (System.Management.ManagementObject obj in searcher.Get())
                        {
                            string id = obj["InstanceName"].ToString() ?? "";
                            monitors.Add(new KeyValuePair<string, string>(id, "Monitor " + (monitors.Count + 1)));
                        }
                    }
                } catch { }

                if (monitors.Count == 0) monitors.Add(new KeyValuePair<string, string>("", "Domyślny Monitor"));
                
                DevicesComboBox.DisplayMemberPath = "Value";
                DevicesComboBox.ItemsSource = monitors;
                DevicesComboBox.SelectedIndex = 0;
            }
            else if (selected is CustomShortcutAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ShortcutSettingsGroup.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "SKRÓT KLAWIATUROWY";
                _capturedKeys.Clear();
                ShortcutTextBox.Text = "KLIKNIJ I NACIŚNIJ KLAWISZE";
                ShortcutTextBox.Focus();
            }
            else if (selected is PasteTextAction pt)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Collapsed;
                ExtraSettingsTitle.Text = "WKLEJANIE TEKSTU";
                InputLabel.Text = "TEKST DO WKLEJENIA";
                PasteTextInput.Text = pt.TextToPaste;
                PasteTextInput.Focus();
            }
            else if (selected is OpenAppAction appAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "OTWIERANIE APLIKACJI";
                InputLabel.Text = "ŚCIEŻKA DO PLIKU .EXE";
                PasteTextInput.Text = appAction.Path;
                PasteTextInput.Focus();
            }
            else if (selected is OpenLinkAction ol)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Collapsed;
                ExtraSettingsTitle.Text = "OTWIERANIE ADRESU URL";
                InputLabel.Text = "ADRES INTERNETOWY";
                PasteTextInput.Text = ol.Url;
                PasteTextInput.Focus();
            }
            else if (selected is CreateFolderAction cf)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "TWORZENIE FOLDERU";
                InputLabel.Text = "ŚCIEŻKA DOCELOWA";
                PasteTextInput.Text = cf.FolderPath;
                PasteTextInput.Focus();
            }
            else if (selected is PowerShellAction ps)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "SKRYPT POWERSHELL";
                InputLabel.Text = "KOMENDA LUB ŚCIEŻKA .PS1";
                PasteTextInput.Text = ps.ScriptContent;
                PasteTextInput.Focus();
            }
            else if (selected is MacroAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                MacroPanel.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "REJESTRATOR MAKRA";
                _macroSteps.Clear();
                MacroStatusText.Text = "GOTOWY DO ZAPISU";
            }
            else if (selected is FanSpeedAction fs)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ValueSliderPanel.Visibility = Visibility.Visible;
                ValueSlider.Maximum = 100;
                ValueSlider.Value = fs.Value;
                ExtraSettingsTitle.Text = "WENTYLATORY";
                SliderLabel.Text = "PRĘDKOŚĆ OBROTOWA (%)";
            }
            else if (selected is RgbColorAction rc)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ValueSliderPanel.Visibility = Visibility.Visible;
                ValueSlider.Maximum = 360;
                ValueSlider.Value = rc.Value;
                ExtraSettingsTitle.Text = "PODŚWIETLENIE RGB";
                SliderLabel.Text = "ODCIEŃ KOLORU (HUE)";
            }
            else if (selected is RgbLightAction rl)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ValueSliderPanel.Visibility = Visibility.Visible;
                ValueSlider.Maximum = 100;
                ValueSlider.Value = rl.Value;
                ExtraSettingsTitle.Text = "JASNOŚĆ LED";
                SliderLabel.Text = "INTENSYWNOŚĆ (%)";
            }
            else if (selected is SoundboardAction sb)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                PasteTextPanel.Visibility = Visibility.Visible;
                BrowseFileButton.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "SOUNDBOARD";
                InputLabel.Text = "PLIK DŹWIĘKOWY (MP3/WAV)";
                PasteTextInput.Text = sb.FilePath;
                PasteTextInput.Focus();
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
            RecordMacroButton.Content = "STOP (LUB NACIŚNIJ PRZYCISK)";
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
                RecordMacroButton.Content = "NAGRAJ";
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
            else if (selected is FanSpeedAction fsAction)
            {
                fsAction.Value = (int)ValueSlider.Value;
                SelectedAction = fsAction;
            }
            else if (selected is RgbColorAction rcAction)
            {
                rcAction.Value = (int)ValueSlider.Value;
                SelectedAction = rcAction;
            }
            else if (selected is RgbLightAction rlAction)
            {
                rlAction.Value = (int)ValueSlider.Value;
                SelectedAction = rlAction;
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
