using System.Collections.Generic;
using System.Threading.Tasks;

namespace KOYA_APP
{
    public class MultiAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Multi-Action";
        public string Icon => "\uE10E"; // Ikona listy / warstw
        public string Category => "Automatyzacja & Narzędzia";
        public string Description => $"{Actions.Count} akcji w sekwencji";
        
        public List<IStreamDeckAction> Actions { get; set; } = new List<IStreamDeckAction>();

        public async void Execute()
        {
            foreach (var action in Actions)
            {
                action.Execute();
                // Krótki odstęp między akcjami, żeby system/aplikacje zdążyły zareagować
                await Task.Delay(100);
            }
        }

        public void ExecuteAnalog(bool direction)
        {
            // Multi-action zazwyczaj nie ma sensu na gałce, ale możemy przekazać do pierwszej akcji analogowej
            foreach (var action in Actions)
            {
                action.ExecuteAnalog(direction);
            }
        public void ExecuteAbsolute(int value) { }
        }
    }
}
