using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace KOYA_APP
{
    public partial class MainWindow : Window
    {
        private IStreamDeckAction?[] _buttonActions = new IStreamDeckAction?[14];
        private Dictionary<string, System.Windows.Controls.Button> _buttonCache = new Dictionary<string, System.Windows.Controls.Button>();
        private System.Windows.Forms.NotifyIcon _notifyIcon = null!;
        private TutorialManager? _tutorialManager;
        private HidBackend _hidBackend = new HidBackend();
        private Dictionary<int, DateTime> _lastAnalogProcessTime = new Dictionary<int, DateTime>();

        private bool _enablePopups = true;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                System.IO.File.WriteAllText("crash_log.txt", e.ExceptionObject.ToString());
            };
            try
            {
                InitializeComponent();
                CacheButtons();
                SetupTrayIcon();
                LoadAndApplyConfig();
                SetupTutorial();
                
                this.Loaded += (s, e) => {
                    _startupTime = DateTime.Now;
                    SetupHid();
                };
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash_init_log.txt", ex.ToString());
                throw;
            }
        }

        private DateTime _startupTime;
        private bool IsAppStable => (DateTime.Now - _startupTime).TotalSeconds > 2;

        private void LoadAndApplyConfig()
        {
            var config = ConfigurationManager.LoadConfig();
            _buttonActions = config.Actions;
            _enablePopups = config.EnablePopups;
            UpdatePopupToggleButtonUI();

            for (int i = 0; i < _buttonActions.Length; i++)
            {
                if (_buttonActions[i] != null)
                {
                    UpdateButtonUI(i, _buttonActions[i]!);
                }
            }
        }

        private void UpdatePopupToggleButtonUI()
        {
            PopupToggleButton.Opacity = _enablePopups ? 1.0 : 0.4;
            PopupToggleButton.ToolTip = _enablePopups ? "Popup Notifications: ON" : "Popup Notifications: OFF";
        }

        private void PopupToggle_Click(object sender, RoutedEventArgs e)
        {
            _enablePopups = !_enablePopups;
            UpdatePopupToggleButtonUI();
            ConfigurationManager.SaveConfig(_buttonActions, enablePopups: _enablePopups);
            
            ShowNotification("\uEA8F", "Notifications", _enablePopups ? "Enabled" : "Disabled");
        }

        private NotificationWindow? _activeNotification;
        private string? _lastNotificationName;

        private void ShowNotification(string icon, string name, string description)
        {
            if (!_enablePopups) return;
            
            Dispatcher.Invoke(() => {
                // Jeśli to ta sama akcja (np. ciągła zmiana głośności), aktualizujemy istniejące okno
                if (_activeNotification != null && _activeNotification.IsLoaded && _lastNotificationName == name)
                {
                    _activeNotification.UpdateContent(description);
                }
                else
                {
                    // Jeśli stare okno jeszcze istnieje ale nazwa inna, zamykamy je
                    if (_activeNotification != null) { try { _activeNotification.Close(); } catch {} }

                    _activeNotification = new NotificationWindow(icon, name, description);
                    _lastNotificationName = name;
                    _activeNotification.Closed += (s, e) => {
                        if (_activeNotification == (NotificationWindow)s!) _activeNotification = null;
                    };
                    _activeNotification.Show();
                }
            });
        }

        private bool ShouldProcessAnalog(int index)
        {
            if (!_lastAnalogProcessTime.ContainsKey(index))
            {
                _lastAnalogProcessTime[index] = DateTime.MinValue;
            }

            // Ograniczamy do max 50 zdarzeń na sekundę (co 20ms)
            if ((DateTime.Now - _lastAnalogProcessTime[index]).TotalMilliseconds < 20)
            {
                return false;
            }

            _lastAnalogProcessTime[index] = DateTime.Now;
            return true;
        }

        private void CacheButtons()
        {
            _buttonCache.Clear();
            foreach (var btn in FindLogicalChildren<System.Windows.Controls.Button>(this))
            {
                if (btn.Tag != null)
                {
                    _buttonCache[btn.Tag.ToString()!] = btn;
                }
            }
        }

        private void PlayClickSound()
        {
            try
            {
                Task.Run(() =>
                {
                    using (var waveOut = new NAudio.Wave.WaveOutEvent())
                    {
                        // Tactile mechanical click: Lower frequency (400Hz), very short (15ms)
                        var signal = new NAudio.Wave.SampleProviders.SignalGenerator(44100, 1)
                        {
                            Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Sin,
                            Frequency = 400,
                            Gain = 0.08
                        }.Take(TimeSpan.FromMilliseconds(15));

                        waveOut.Init(signal);
                        waveOut.Play();
                        while (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                        {
                            Thread.Sleep(2);
                        }
                    }
                });
            }
            catch { }
        }

        private bool _isPickerOpen = false;
        private void OpenPickerForIndex(int index)
        {
            if (index < 0 || index >= _buttonActions.Length) return;
            if (_isPickerOpen) return; // Zapobiegaj otwieraniu wielu okien na raz

            _isPickerOpen = true;
            try
            {
                bool isAnalog = index >= 12;
                ActionPicker picker = new ActionPicker(index, _hidBackend, isAnalog) { Owner = this };
                
                if (picker.ShowDialog() == true)
                {
                    _buttonActions[index] = picker.SelectedAction!;
                    UpdateButtonUI(index, _buttonActions[index]!);
                    ConfigurationManager.SaveConfig(_buttonActions);
                }
            }
            finally
            {
                _isPickerOpen = false;
            }
        }

        private CancellationTokenSource? _connectionCts;

        private DateTime _lastConnectionTime = DateTime.MinValue;
        private bool IsHardwareStable => (DateTime.Now - _lastConnectionTime).TotalSeconds > 2;

        private void SetupHid()
        {
            _hidBackend.ButtonPressed += (index) =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    if (index >= 0 && index < _buttonActions.Length)
                    {
                        var action = _buttonActions[index];
                        
                        if (action != null)
                        {
                            action.Execute();
                            PlayClickSound();
                            ShowNotification(action.Icon, action.Name, "Akcja wykonana");
                        }
                        
                        if (_buttonCache.TryGetValue(index.ToString(), out var btn))
                        {
                            var originalBackground = btn.Background;
                            var highlightBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(120, 255, 255, 255));
                            btn.Background = highlightBrush;
                            
                            var anim = new System.Windows.Media.Animation.ColorAnimation(
                                System.Windows.Media.Colors.Transparent, 
                                System.TimeSpan.FromMilliseconds(200));
                            btn.Background.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty, anim);
                            
                            await System.Threading.Tasks.Task.Delay(200);
                            btn.Background = originalBackground;
                        }
                    }
                }));
            };

            _hidBackend.KnobAbsoluteChanged += (index, value) =>
            {
                if (!IsHardwareStable) return;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (index >= 0 && index < _buttonActions.Length)
                    {
                        var action = _buttonActions[index];
                        if (action != null)
                        {
                            action.ExecuteAbsolute(value);
                        }
                        
                        // Update UI Knob rotation and Text
                        if (_buttonCache.TryGetValue(index.ToString(), out var btn))
                        {
                            btn.ApplyTemplate();
                            if (btn.Template != null)
                            {
                                // 1. Update Pointer Rotation
                                var transform = btn.Template.FindName("pointer", btn) as FrameworkElement;
                                if (transform != null)
                                {
                                    double angle = (value / 255.0) * 300.0 - 150.0;
                                    transform.RenderTransform = new System.Windows.Media.RotateTransform(angle);
                                }

                                // 2. Update Percentage Text
                                var valueText = btn.Template.FindName("valueText", btn) as TextBlock;
                                if (valueText != null)
                                {
                                    int percentage = (int)(value / 255.0 * 100.0);
                                    valueText.Text = $"{percentage}%";

                                    // Wyślij powiadomienie (ShowNotification zajmie się uniknięciem spamu)
                                    ShowNotification(action.Icon, action.Name, $"{percentage}%");
                                }                            }
                        }
                    }
                }));
            };

            _hidBackend.KnobTurned += (index, direction) =>
            {
                // To zostawiamy tylko dla enkoderów jeśli by były (obecnie nieużywane dla potów)
                if (!IsHardwareStable) return;
                // ... logic for relative encoders ...
            };

            _hidBackend.ConnectionStatusChanged += (isConnected) =>
            {
                if (isConnected) _lastConnectionTime = DateTime.Now;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    _connectionCts?.Cancel();
                    _connectionCts = new CancellationTokenSource();
                    var token = _connectionCts.Token;

                    if (!isConnected)
                    {
                        try 
                        { 
                            await System.Threading.Tasks.Task.Delay(500, token); 
                            UpdateStatusUI(false);
                        } 
                        catch (OperationCanceledException) { }
                    }
                    else
                    {
                        UpdateStatusUI(true);
                    }
                }));
            };

            _hidBackend.Start();
        }

        private void UpdateStatusUI(bool isConnected)
        {
            var statusDot = this.FindName("StatusDot") as System.Windows.Shapes.Ellipse;
            var statusText = this.FindName("StatusText") as TextBlock;
            var disconnectedOverlay = this.FindName("DisconnectedOverlay") as Grid;

            if (statusDot != null)
                statusDot.Fill = isConnected ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.DimGray;
            
            if (statusText != null)
                statusText.Text = isConnected ? "CONNECTED" : "DISCONNECTED";

            if (disconnectedOverlay != null)
                disconnectedOverlay.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ClearAction(int index)
        {
            if (index < 0 || index >= _buttonActions.Length) return;
            
            _buttonActions[index] = null;
            var btn = FindButtonByTag(index.ToString());
            if (btn != null)
            {
                btn.Content = index >= 12 ? "\uE995;" : "\uE710;"; // Reset ikony na "+" lub "Knob"
                btn.ToolTip = "Kliknij aby przypisać akcję";
            }
            ConfigurationManager.SaveConfig(_buttonActions);
        }

        private void SetupTutorial()
        {
            _tutorialManager = new TutorialManager(RootGrid);

            _tutorialManager.AddStep(this, "Witaj w KOYA!", "Ten krótki przewodnik pokaże Ci, jak skonfigurować Twoją konsolę sterowania.");

            var firstButton = FindButtonByTag("0");
            if (firstButton != null)
            {
                _tutorialManager.AddStep(firstButton, "Dodawanie Akcji", "Kliknij dowolny pusty przycisk, aby otworzyć menu wyboru akcji. Możesz tam przypisać np. 'Otwórz Aplikację' lub 'Skrót Klawiszowy'.");
                _tutorialManager.AddStep(firstButton, "Media Control", "Szukasz bindowania Play/Pause? Wybierz 'Media Control' z listy, a następnie naciśnij 'Zastosuj'. Ikona zmieni się automatycznie!");
            }

            var firstKnob = FindButtonByTag("12");
            if (firstKnob != null)
                _tutorialManager.AddStep(firstKnob, "Gałki Analogowe", "Gałki służą do precyzyjnej kontroli. Przewijaj kółkiem myszy nad gałką, aby np. płynnie zmieniać głośność systemu.");

            _tutorialManager.AddStep(this, "Gotowe!", "To wszystko! Po zakończeniu tego samouczka, kliknij dowolny przycisk i stwórz swój własny zestaw narzędzi. Miłej zabawy!");
        }

        private void Tutorial_Click(object sender, RoutedEventArgs e)
        {
            _tutorialManager?.Start();
        }

        private void UpdateButtonUI(int index, IStreamDeckAction action)
        {
            var btn = FindButtonByTag(index.ToString());
            if (btn == null) return;

            btn.ToolTip = action.Name;

            // Handle Knob (index 12-13) preview
            if (index >= 12)
            {
                btn.Content = action.Icon;
                btn.ApplyTemplate();
                
                if (btn.Template != null)
                {
                    var actionNameText = btn.Template.FindName("actionNameText", btn) as TextBlock;
                    if (actionNameText != null)
                    {
                        actionNameText.Text = action.Name.ToUpper();
                    }
                    var valueText = btn.Template.FindName("valueText", btn) as TextBlock;
                    if (valueText != null && valueText.Text == "--")
                    {
                        valueText.Text = "READY"; // Pokazuje, że akcja jest przypisana
                    }
                }
                return;
            }

            if (action is OpenAppAction appAction && !string.IsNullOrEmpty(appAction.Path))
            {
                try
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(appAction.Path);
                    if (icon != null)
                    {
                        var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        btn.Content = new System.Windows.Controls.Image { Source = imageSource, Stretch = System.Windows.Media.Stretch.Uniform, Margin = new Thickness(15) };
                        return;
                    }
                }
                catch { }
            }
            
            btn.Content = action.Icon;
        }

        private System.Windows.Controls.Button? FindButtonByTag(string tag)
        {
            return FindLogicalChildren<System.Windows.Controls.Button>(this)
                .FirstOrDefault(b => b.Tag?.ToString() == tag);
        }

        private IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object child in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (child is T t) yield return t;
                    if (child is DependencyObject d)
                    {
                        foreach (T childOfChild in FindLogicalChildren<T>(d)) yield return childOfChild;
                    }
                }
            }
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            try
            {
                var streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/logo.png"));
                if (streamResourceInfo != null)
                {
                    using (var stream = streamResourceInfo.Stream)
                    {
                        var bitmap = new System.Drawing.Bitmap(stream);
                        IntPtr hIcon = bitmap.GetHicon();
                        _notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
                    }
                }
                else
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch { _notifyIcon.Icon = System.Drawing.SystemIcons.Application; }

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "KOYA";
            _notifyIcon.DoubleClick += (s, e) => ShowWindow();

            var contextMenu = new System.Windows.Forms.ContextMenuStrip
            {
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                ForeColor = System.Drawing.Color.White,
                ShowImageMargin = false,
                RenderMode = System.Windows.Forms.ToolStripRenderMode.System,
                Font = new System.Drawing.Font("Segoe UI", 9f)
            };

            var showItem = new System.Windows.Forms.ToolStripMenuItem("Pokaż Konsole");
            showItem.Click += (s, e) => ShowWindow();
            
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Zakończ KOYA");
            exitItem.Click += (s, e) => ShutdownApp();

            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator { BackColor = System.Drawing.Color.FromArgb(40, 40, 40) });
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true;
        }

        private void OnDeckButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn.Tag == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int index)) return;
            if (index < 0 || index >= _buttonActions.Length) return;

            // Lewy klik w UI teraz zawsze otwiera okno konfiguracji, nie wykonuje akcji.
            OpenPickerForIndex(index);
        }

        private void OnDeckButtonRightClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn.Tag == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int index)) return;
            if (index < 0 || index >= _buttonActions.Length) return;

            var action = _buttonActions[index];
            if (action != null)
            {
                // Prawy klik w UI teraz wykonuje akcję (do testów)
                action.Execute();
                PlayClickSound();
                ShowNotification(action.Icon, action.Name, "Akcja wykonana (UI)");
            }
            else
            {
                // Jeśli puste, otwórz picker
                OpenPickerForIndex(index);
            }
        }

        private void OnKnobMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int index)) return;

            if (_buttonActions[index] != null)
            {
                bool direction = e.Delta > 0;
                _buttonActions[index].ExecuteAnalog(direction);
                ShowNotification(_buttonActions[index].Icon, _buttonActions[index].Name, direction ? "Zwiększono / Następny" : "Zmniejszono / Poprzedni");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Hide();
        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isShuttingDown)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnClosing(e);
        }

        private bool _isShuttingDown = false;
        private void ShutdownApp()
        {
            _isShuttingDown = true;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void DisconnectedOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            DisconnectedOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
