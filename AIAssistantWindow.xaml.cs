using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace KOYA_APP
{
    public partial class AIAssistantWindow : Window
    {
        private bool _isExpanded = false;
        private ObservableCollection<string> _attachedFiles = new ObservableCollection<string>();
        private Storyboard? _thinkingAnim;
        private string? _apiKey;
        private string? _groqKey;
        private string? _openAIKey;
        private AIProvider _currentProvider = AIProvider.Google;
        private byte[]? _lastScreenshot;
        private List<string> _fullFilePaths = new List<string>();

        public AIAssistantWindow()
        {
            InitializeComponent();
            this.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
            AttachedFilesList.ItemsSource = _attachedFiles;
            _thinkingAnim = (Storyboard)this.Resources["ThinkingAnimation"];

            LoadConfigKeys();
        }

        private void LoadConfigKeys()
        {
            var config = ConfigurationManager.LoadConfig();
            _apiKey = config.GeminiKey;
            _groqKey = config.GroqKey;
            _openAIKey = config.OpenAIKey;

            ApiKeyInput.Text = _apiKey;
            GroqKeyInput.Text = _groqKey;
            OpenAIKeyInput.Text = _openAIKey;

            if (string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(_groqKey) && string.IsNullOrEmpty(_openAIKey))
            {
                this.Loaded += (s, e) => {
                    ExpandWindow();
                    SetupView.Visibility = Visibility.Visible;
                };
            }
        }

        private void SaveKey_Click(object sender, RoutedEventArgs e)
        {
            _apiKey = ApiKeyInput.Text.Trim();
            _groqKey = GroqKeyInput.Text.Trim();
            _openAIKey = OpenAIKeyInput.Text.Trim();

            var config = ConfigurationManager.LoadConfig();
            ConfigurationManager.SaveConfig(config.Actions, config.IsFirstStart, _apiKey, config.EnablePopups, _groqKey, _openAIKey);

            SetupView.Visibility = Visibility.Collapsed;
            ResponseText.Text = "Ustawienia AI zapisane! Możesz teraz korzystać z asystenta.";
        }

        private void ResetKey_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.LoadConfig();
            ConfigurationManager.SaveConfig(config.Actions, config.IsFirstStart, "", config.EnablePopups, "", "");
            
            _apiKey = _groqKey = _openAIKey = null;
            ApiKeyInput.Clear();
            GroqKeyInput.Clear();
            OpenAIKeyInput.Clear();
            
            SetupView.Visibility = Visibility.Visible;
            ResponseText.Text = "Klucze AI zostały usunięte.";
        }

        private void ProviderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProviderSelector == null) return;
            if (ProviderSelector.SelectedIndex == 0) _currentProvider = AIProvider.Google;
            else if (ProviderSelector.SelectedIndex == 1) _currentProvider = AIProvider.Groq;
            else if (ProviderSelector.SelectedIndex == 2) _currentProvider = AIProvider.OpenAI;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SetupView.Visibility = SetupView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void PromptInput_GotFocus(object sender, RoutedEventArgs e) { }

        private void PromptInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Send_Click(this, new RoutedEventArgs());
            }
        }

        private void ExpandWindow()
        {
            if (_isExpanded) return;
            _isExpanded = true;

            DoubleAnimation heightAnim = new DoubleAnimation(450, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation widthAnim = new DoubleAnimation(650, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(Window.HeightProperty, heightAnim);
            this.BeginAnimation(Window.WidthProperty, widthAnim);

            ExpandedToolbar.Visibility = Visibility.Visible;
            ExpandedView.Visibility = Visibility.Visible;
            
            this.Top -= 195;
            this.Left -= 125;
        }

        private void StartThinking()
        {
            ThinkingOverlay.Visibility = Visibility.Visible;
            _thinkingAnim?.Begin();
        }

        private void StopThinking()
        {
            _thinkingAnim?.Stop();
            ThinkingOverlay.Visibility = Visibility.Collapsed;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            string prompt = PromptInput.Text;
            if (string.IsNullOrWhiteSpace(prompt) && _attachedFiles.Count == 0 && _lastScreenshot == null) return;

            if (!_isExpanded) ExpandWindow();

            ResponseText.Text = $"KOYA AI ({_currentProvider}) analizuje zapytanie...";
            StartThinking();

            try
            {
                var service = new AIService(_apiKey, _groqKey, _openAIKey);
                string response = await service.GetAIResponse(prompt, _fullFilePaths, _lastScreenshot, _currentProvider);
                
                Dispatcher.Invoke(() => {
                    StopThinking();
                    ResponseText.Text = response;
                    PromptInput.Clear();
                    _attachedFiles.Clear();
                    _fullFilePaths.Clear();
                    _lastScreenshot = null;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                    StopThinking();
                    ResponseText.Text = "Wystąpił błąd: " + ex.Message;
                });
            }
        }

        private void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Title = "Wybierz dokumenty do Gemini"
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!_fullFilePaths.Contains(file))
                    {
                        _fullFilePaths.Add(file);
                        _attachedFiles.Add(System.IO.Path.GetFileName(file));
                    }
                }
                if (!_isExpanded) ExpandWindow();
            }
        }

        private void Mic_Click(object sender, RoutedEventArgs e)
        {
            if (!_isExpanded) ExpandWindow();
            ResponseText.Text = "Słucham...";
            StartThinking();
            Task.Run(async () => {
                await Task.Delay(2000);
                Dispatcher.Invoke(StopThinking);
            });
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Task.Run(async () =>
            {
                await Task.Delay(400);
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        double screenLeft = SystemParameters.VirtualScreenLeft;
                        double screenTop = SystemParameters.VirtualScreenTop;
                        double screenWidth = SystemParameters.VirtualScreenWidth;
                        double screenHeight = SystemParameters.VirtualScreenHeight;

                        using (Bitmap bmp = new Bitmap((int)screenWidth, (int)screenHeight))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                            }
                            
                            using (var ms = new MemoryStream())
                            {
                                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                _lastScreenshot = ms.ToArray();
                            }

                            if (!_isExpanded) ExpandWindow();
                            ResponseText.Text = "Zrzut ekranu gotowy dla Gemini.";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!_isExpanded) ExpandWindow();
                        ResponseText.Text = "Błąd: " + ex.Message;
                    }
                    this.Show();
                });
            });
        }
    }
}
