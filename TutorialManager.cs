using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace KOYA_APP
{
    public class TutorialStep
    {
        public FrameworkElement Element { get; set; } = null!;
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class TutorialManager
    {
        private readonly Canvas _tutorialOverlay;
        private readonly Grid _container;
        private readonly List<TutorialStep> _steps = new();
        private int _currentStepIndex = -1;
        private System.Windows.Shapes.Rectangle? _highlightRect;
        private Border? _tooltipBorder;
        private TextBlock? _titleText;
        private TextBlock? _descText;

        public TutorialManager(Grid container)
        {
            _container = container;
            _tutorialOverlay = new Canvas
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)),
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = true
            };
            Grid.SetRowSpan(_tutorialOverlay, 10);
            Grid.SetColumnSpan(_tutorialOverlay, 10);
            _container.Children.Add(_tutorialOverlay);

            _tutorialOverlay.MouseDown += (s, e) => NextStep();
        }

        public void AddStep(FrameworkElement element, string title, string description)
        {
            _steps.Add(new TutorialStep { Element = element, Title = title, Description = description });
        }

        public void Start()
        {
            if (_steps.Count == 0) return;
            _tutorialOverlay.Visibility = Visibility.Visible;
            _currentStepIndex = 0;
            ShowCurrentStep();
        }

        private void NextStep()
        {
            _currentStepIndex++;
            if (_currentStepIndex >= _steps.Count)
            {
                End();
            }
            else
            {
                ShowCurrentStep();
            }
        }

        private void ShowCurrentStep()
        {
            _tutorialOverlay.Children.Clear();
            var step = _steps[_currentStepIndex];

            // Safety check for visual tree
            if (!step.Element.IsLoaded || !step.Element.IsDescendantOf(_container))
            {
                // If element is not in tree or not a descendant, we can't highlight it properly
                // Fallback: show description in the middle of the screen
                ShowFallbackStep(step);
                return;
            }

            try
            {
                // Get element position relative to container
                var transform = step.Element.TransformToAncestor(_container);
                var rect = transform.TransformBounds(new Rect(0, 0, step.Element.ActualWidth, step.Element.ActualHeight));

                // Create highlight hole
                var geometryGroup = new GeometryGroup();
                geometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, _container.ActualWidth, _container.ActualHeight)));
                geometryGroup.Children.Add(new RectangleGeometry(rect));
                _tutorialOverlay.Background = new DrawingBrush(new GeometryDrawing(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, geometryGroup));

                CreateTooltip(step, rect);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tutorial error: {ex.Message}");
                ShowFallbackStep(step);
            }
        }

        private void ShowFallbackStep(TutorialStep step)
        {
            _tutorialOverlay.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
            CreateTooltip(step, new Rect(_container.ActualWidth/2 - 5, _container.ActualHeight/2 - 5, 10, 10));
        }

        private void CreateTooltip(TutorialStep step, Rect rect)
        {
            // Tooltip
            _tooltipBorder = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20)),
                BorderBrush = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Width = 300,
                CornerRadius = new CornerRadius(8)
            };

            var stack = new StackPanel();
            _titleText = new TextBlock 
            { 
                Text = step.Title, 
                Foreground = System.Windows.Media.Brushes.White, 
                FontSize = 18, 
                FontWeight = FontWeights.Bold, 
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap,
                FontFamily = (System.Windows.Media.FontFamily)System.Windows.Application.Current.Resources["MainFont"]
            };
            _descText = new TextBlock 
            { 
                Text = step.Description, 
                Foreground = System.Windows.Media.Brushes.Gray, 
                FontSize = 14, 
                TextWrapping = TextWrapping.Wrap 
            };
            
            stack.Children.Add(_titleText);
            stack.Children.Add(_descText);
            stack.Children.Add(new TextBlock 
            { 
                Text = "Kliknij gdziekolwiek, aby kontynuować...", 
                Foreground = System.Windows.Media.Brushes.DimGray, 
                FontSize = 10, 
                Margin = new Thickness(0, 15, 0, 0) 
            });

            _tooltipBorder.Child = stack;
            _tutorialOverlay.Children.Add(_tooltipBorder);

            // Position tooltip
            double top = rect.Bottom + 20;
            double left = rect.Left + (rect.Width / 2) - 150;

            if (top + 150 > _container.ActualHeight) top = rect.Top - 170;
            if (left < 10) left = 10;
            if (left + 310 > _container.ActualWidth) left = _container.ActualWidth - 310;
            
            // Final safety for bounds
            if (top < 0) top = 10;

            Canvas.SetTop(_tooltipBorder, top);
            Canvas.SetLeft(_tooltipBorder, left);

            // Animation
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            _tooltipBorder.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void End()
        {
            _tutorialOverlay.Visibility = Visibility.Collapsed;
            _currentStepIndex = -1;
        }
    }
}
