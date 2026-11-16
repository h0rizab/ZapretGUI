using System.IO;
using System.ComponentModel;
using System.Windows;

namespace ZapretGui
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var app = (App)Application.Current;

            if (!app.IsExiting)
            {
                if (app.State.Settings.MinimizeToTray)
                {
                    // Р С”РЎР‚Р ВµРЎРѓРЎвЂљР С‘Р С” РЎРѓР Р†Р С•РЎР‚Р В°РЎвЂЎР С‘Р Р†Р В°Р ВµРЎвЂљ Р Р† РЎвЂљРЎР‚Р ВµР в„–
                    e.Cancel = true;
                    Hide();
                    app.NotifyHiddenToTray();
                }
                else
                {
                    app.ExitApplication();
                }
            }

            base.OnClosing(e);
        }
    }
}

