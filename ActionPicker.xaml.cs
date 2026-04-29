using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
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

            // Media
            actions.Add(new PlayPauseAction());
            actions.Add(new NextTrackAction());
            actions.Add(new PreviousTrackAction());
            actions.Add(new VolumeAction());
            
            // System
            actions.Add(new CopyAction());
            actions.Add(new PasteAction());
            actions.Add(new ScreenshotAction());
            actions.Add(new TaskManagerAction());
            actions.Add(new CloseWindowAction());
            actions.Add(new FullscreenAction());
            actions.Add(new AltTabAction());

            // Zaawansowane
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
                    // Uzylem prostego MessageBoxa zamiast InputBoxa, bo Microsoft.VisualBasic moze nie byc podpiety
                    MessageBox.Show("Funkcja wyboru mikrofonu wymaga rozbudowanego UI. Na razie wybieram pierwszy dostepny.");
                    if (deviceList.Count > 0)
                    {
                        micAction.DeviceID = deviceList[0].ID;
                        micAction.DeviceName = deviceList[0].FriendlyName;
                        SelectedAction = micAction;
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                catch { MessageBox.Show("Blad mikrofonu!"); }
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
