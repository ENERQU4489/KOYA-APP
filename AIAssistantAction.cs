using System.Windows;

namespace KOYA_APP
{
    public class AIAssistantAction : IStreamDeckAction
    {
        public string Name => "AI Assistant";
        public string Description => "Otwórz okno asystenta AI";
        public string Icon => "\uE946"; // Info/Brain icon

        public void Execute()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var assistantWindow = new AIAssistantWindow();
                assistantWindow.Show();
                assistantWindow.Activate();
            });
        }

        public void ExecuteAnalog(bool direction) { }
    }
}
