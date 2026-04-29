using Microsoft.Win32;
using NAudio.CoreAudioApi; // To zadziała po instalacji NuGet NAudio
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace KOYA_APP
{
    public partial class ActionPicker : Window
    {
        public IStreamDeckAction SelectedAction { get; set; }

        public ActionPicker()
        {
            InitializeComponent();

            var actions = new List<IStreamDeckAction>();

            // Standardowe
            actions.Add(new PlayPauseAction());
            actions.Add(new NextTrackAction());
            actions.Add(new PreviousTrackAction());
            actions.Add(new VolumeAction());
            actions.Add(new CopyAction());
            actions.Add(new PasteAction());

            // Nowe - jeśli nadal podkreśla na czerwono, sprawdź czy pliki .cs są w projekcie!
            actions.Add(new ScreenshotAction());
            actions.Add(new OpenAppAction());
            actions.Add(new MuteMicrophoneAction());
            actions.Add(new SelectMicAction());

            ActionsListBox.ItemsSource = actions;
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
                    this.DialogResult = true;
                    this.Close();
                }
            }
            else if (selected is SelectMicAction micAction)
            {
                try
                {
                    var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                    var deviceList = new List<MMDevice>();
                    string menuText = "Wybierz numer mikrofonu:\n";
                    int i = 0;
                    foreach (var d in devices)
                    {
                        menuText += $"{i}: {d.FriendlyName}\n";
                        deviceList.Add(d);
                        i++;
                    }
                    string input = Microsoft.VisualBasic.Interaction.InputBox(menuText, "Wybór Mikrofonu", "0");
                    if (int.TryParse(input, out int index) && index >= 0 && index < deviceList.Count)
                    {
                        micAction.DeviceID = deviceList[index].ID;
                        micAction.DeviceName = deviceList[index].FriendlyName;
                        SelectedAction = micAction;
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                catch { MessageBox.Show("Błąd mikrofonu!"); }
            }
            else
            {
                SelectedAction = selected;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}