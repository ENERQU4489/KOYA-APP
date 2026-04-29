using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KOYA_APP
{
    public partial class ActionPicker : Window
    {
        public IStreamDeckAction? SelectedAction { get; set; }
        private List<byte> _capturedKeys = new List<byte>();

        public ActionPicker()
        {
            InitializeComponent();

            var actions = new List<IStreamDeckAction>
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
                new CustomShortcutAction(), // NOWOSC
                new OpenAppAction(),
                new MuteMicrophoneAction(),
                new MuteSpeakerAction(),
                new SelectMicAction()
            };

            ActionsListBox.ItemsSource = actions;
        }

        private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            
            ExtraSettingsPanel.Visibility = Visibility.Collapsed;
            DevicesComboBox.Visibility = Visibility.Collapsed;
            ShortcutTextBox.Visibility = Visibility.Collapsed;

            if (selected is SelectMicAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                DevicesComboBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wybierz urzadzenie wejsciowe (Mikrofon):";
                
                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
                DevicesComboBox.ItemsSource = devices;
                if (devices.Count > 0) DevicesComboBox.SelectedIndex = 0;
            }
            else if (selected is CustomShortcutAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ShortcutTextBox.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Wcisnij kombinacje klawiszy:";
                _capturedKeys.Clear();
                ShortcutTextBox.Text = "Kliknij tutaj i wcisnij klawisze...";
            }
            else if (selected is OpenAppAction)
            {
                ExtraSettingsPanel.Visibility = Visibility.Visible;
                ExtraSettingsTitle.Text = "Sciezka zostanie wybrana po kliknieciu 'Zatwierdz'";
            }
        }

        private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            
            // Konwertujemy klawisz WPF na VirtualKey (Win32)
            int vk = KeyInterop.VirtualKeyFromKey(e.Key);
            if (vk == 0) return;

            byte vkByte = (byte)vk;

            if (!_capturedKeys.Contains(vkByte))
            {
                _capturedKeys.Add(vkByte);
            }

            // Wyswietlamy nazwy klawiszy
            List<string> keyNames = new List<string>();
            foreach (var k in _capturedKeys)
            {
                keyNames.Add(((Key)KeyInterop.KeyFromVirtualKey(k)).ToString());
            }
            ShortcutTextBox.Text = string.Join(" + ", keyNames);
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            var selected = ActionsListBox.SelectedItem as IStreamDeckAction;
            if (selected == null) return;

            if (selected is OpenAppAction appAction)
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Aplikacje (*.exe)|*.exe|Wszystkie pliki (*.*)|*.*" };
                if (ofd.ShowDialog() == true)
                {
                    appAction.Path = ofd.FileName;
                    SelectedAction = appAction;
                }
            }
            else if (selected is CustomShortcutAction shortcutAction)
            {
                if (_capturedKeys.Count > 0)
                {
                    shortcutAction.KeyCodes = new List<byte>(_capturedKeys);
                    shortcutAction.KeysDisplay = ShortcutTextBox.Text;
                    SelectedAction = shortcutAction;
                }
                else
                {
                    MessageBox.Show("Najpierw wcisnij jakies klawisze!");
                    return;
                }
            }
            else if (selected is SelectMicAction micAction)
            {
                var device = DevicesComboBox.SelectedItem as MMDevice;
                if (device != null)
                {
                    micAction.DeviceID = device.ID;
                    micAction.DeviceName = device.FriendlyName;
                    SelectedAction = micAction;
                }
            }
            else
            {
                SelectedAction = selected;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
