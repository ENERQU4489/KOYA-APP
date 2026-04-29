using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace StreamDeckApp
{
    public partial class MainWindow : Window
    {
        private IStreamDeckAction[] _buttonActions = new IStreamDeckAction[12];
        private readonly VolumeAction _volume = new VolumeAction();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnDeckButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            // Sprawdzamy czy Tag nie jest pusty, żeby uniknąć błędu CS8604
            if (btn.Tag == null) return;

            int index = int.Parse(btn.Tag.ToString());

            if (_buttonActions[index] == null)
            {
                ActionPicker picker = new ActionPicker { Owner = this };
                if (picker.ShowDialog() == true)
                {
                    // Tutaj przypisujemy wybraną akcję
                    _buttonActions[index] = picker.SelectedAction;
                    btn.Content = _buttonActions[index].Name;
                }
            }
            else
            {
                _buttonActions[index].Execute();
            }
        }

        private void VolumeUp_Click(object sender, RoutedEventArgs e) => _volume.ChangeVolume(true);
        private void VolumeDown_Click(object sender, MouseButtonEventArgs e) => _volume.ChangeVolume(false);
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
    }
}