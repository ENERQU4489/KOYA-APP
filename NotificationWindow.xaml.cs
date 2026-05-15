using System;
using System.Windows;
using System.Windows.Threading;

namespace KOYA_APP
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer _closeTimer;

        public NotificationWindow(string icon, string name, string description)
        {
            InitializeComponent();
            
            IconText.Text = icon;
            ActionName.Text = name;
            ActionDesc.Text = description;

            // Position in top-right corner
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            this.Left = screenWidth - this.Width - 20;
            this.Top = 40;

            _closeTimer = new DispatcherTimer();
            _closeTimer.Interval = TimeSpan.FromSeconds(3);
            _closeTimer.Tick += (s, e) => {
                _closeTimer.Stop();
                this.Close();
            };
            _closeTimer.Start();
        }
    }
}
