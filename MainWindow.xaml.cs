using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace KOYA_APP
{
    public partial class MainWindow : Window
    {
        private IStreamDeckAction?[] _buttonActions = new IStreamDeckAction?[14];
        private System.Windows.Forms.NotifyIcon _notifyIcon = null!;
        private TutorialManager? _tutorialManager;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                System.IO.File.WriteAllText("crash_log.txt", e.ExceptionObject.ToString());
            };
            try
            {
                InitializeComponent();
                SetupTrayIcon();
                LoadAndApplyConfig();
                SetupTutorial();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash_init_log.txt", ex.ToString());
                throw;
            }
        }

        private void SetupTutorial()
        {
            // Znajdz Grid nadrzedny dla overlay
            var rootGrid = (Grid)this.Content;
            _tutorialManager = new TutorialManager(rootGrid);

            // Dodaj kroki
            var firstButton = FindButtonByTag("0");
            if (firstButton != null)
                _tutorialManager.AddStep(firstButton, "Digital Buttons", "These 12 buttons are for digital actions like opening apps, running macros, or system shortcuts.");

            var firstKnob = FindButtonByTag("12");
            if (firstKnob != null)
                _tutorialManager.AddStep(firstKnob, "Analog Knobs", "Knobs support MouseWheel events. Perfect for volume control, zooming, or track seeking.");

            var syncBtn = FindLogicalChildren<System.Windows.Controls.Button>(this).FirstOrDefault(b => b.ToolTip?.ToString() == "Sync config with GitHub");
            if (syncBtn != null)
                _tutorialManager.AddStep(syncBtn, "GitHub Sync", "Safely backup and sync your configuration to a GitHub repository with one click.");
        }

        private void Tutorial_Click(object sender, RoutedEventArgs e)
        {
            _tutorialManager?.Start();
        }

        private void LoadAndApplyConfig()
        {
            var config = ConfigurationManager.LoadConfig();
            _buttonActions = config.Actions;
            
            // Odswiez UI dla kazdego przycisku
            for (int i = 0; i < _buttonActions.Length; i++)
            {
                if (_buttonActions[i] != null)
                {
                    UpdateButtonUI(i, _buttonActions[i]!);
                }
            }
        }

        private void UpdateButtonUI(int index, IStreamDeckAction action)
        {
            // Znajdz przycisk po Tagu
            var btn = FindButtonByTag(index.ToString());
            if (btn == null) return;

            if (action is OpenAppAction appAction && !string.IsNullOrEmpty(appAction.Path))
            {
                try
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(appAction.Path);
                    if (icon != null)
                    {
                        var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        btn.Content = new System.Windows.Controls.Image { Source = imageSource, Stretch = System.Windows.Media.Stretch.Uniform };
                    }
                }
                catch { btn.Content = action.Icon; }
            }
            else
            {
                btn.Content = action.Icon;
            }
            btn.ToolTip = action.Name;
        }

        private System.Windows.Controls.Button? FindButtonByTag(string tag)
        {
            // Przeszukaj cale drzewo wizualne w poszukiwaniu przycisku z danym tagiem
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
                var streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri("Assets/logo.png", UriKind.Relative));
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

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("POKAZ", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("WYJDZ", null, (s, e) => ShutdownApp());
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true; // Upewniamy sie ze wskoczy na wierzch
        }

        private void OnDeckButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn.Tag == null) return;

            if (!int.TryParse(btn.Tag.ToString(), out int index)) return;
            if (index < 0 || index >= _buttonActions.Length) return;

            if (_buttonActions[index] == null)
            {
                bool isAnalog = index >= 12;
                ActionPicker picker = new ActionPicker(isAnalog) { Owner = this };
                if (picker.ShowDialog() == true)
                {
                    _buttonActions[index] = picker.SelectedAction!;
                    UpdateButtonUI(index, _buttonActions[index]!);
                    ConfigurationManager.SaveConfig(_buttonActions);
                }
            }
            else
            {
                _buttonActions[index].Execute();
            }
        }

        private void OnKnobMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int index)) return;

            if (_buttonActions[index] != null)
            {
                _buttonActions[index].ExecuteAnalog(e.Delta > 0);
            }
        }

        private async void SyncGitHub_Click(object sender, RoutedEventArgs e)
        {
            var btn = (System.Windows.Controls.Button)sender;
            btn.IsEnabled = false;
            
            try
            {
                var result = await GitService.SyncConfigAsync();
                
                if (result.Success)
                {
                    _notifyIcon.ShowBalloonTip(3000, "KOYA Sync", result.Message, System.Windows.Forms.ToolTipIcon.Info);
                }
                else
                {
                    _notifyIcon.ShowBalloonTip(5000, "KOYA Sync Error", result.Message, System.Windows.Forms.ToolTipIcon.Error);
                    System.Windows.MessageBox.Show(result.Message, "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
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
    }
}
