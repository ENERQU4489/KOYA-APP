using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace KOYA_APP
{
    public class SpotifyLikeAction : IStreamDeckAction
    {
        public string Name => "Spotify: Polub";
        public string Description => "Lubię to! (Spotify)";
        public string Icon => "\uEB52"; // Heart icon
        public string Category => "Multimedia & Audio";

        public void Execute()
        {
            try
            {
                // Próbujemy przez UI Automation (działa w tle)
                var spotifyProcess = Process.GetProcessesByName("Spotify")
                    .FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle));

                if (spotifyProcess != null)
                {
                    AutomationElement spotifyWindow = AutomationElement.FromHandle(spotifyProcess.MainWindowHandle);
                    if (spotifyWindow != null)
                    {
                        Condition likeCondition = new OrCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "Add to Liked Songs"),
                            new PropertyCondition(AutomationElement.NameProperty, "Remove from Liked Songs"),
                            new PropertyCondition(AutomationElement.NameProperty, "Polub"),
                            new PropertyCondition(AutomationElement.NameProperty, "Usuń z polubionych")
                        );

                        AutomationElement likeButton = spotifyWindow.FindFirst(TreeScope.Descendants, likeCondition);

                        if (likeButton != null)
                        {
                            var invokePattern = likeButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                            invokePattern?.Invoke();
                            return;
                        }
                    }
                }

                // Fallback: Skrót klawiszowy (wymaga fokusu, ale Spotify często go łapie)
                // Alt + Shift + B
                keybd_event(0x12, 0, 0, 0); // Alt
                keybd_event(0xA0, 0, 0, 0); // LShift
                keybd_event(0x42, 0, 0, 0); // B
                
                keybd_event(0x42, 0, 2, 0);
                keybd_event(0xA0, 0, 2, 0);
                keybd_event(0x12, 0, 2, 0);
            }
            catch
            {
                // Silent fail
            }
        }

        public void ExecuteAnalog(bool direction) { }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
