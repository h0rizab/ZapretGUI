using System.Windows.Controls;

namespace ZapretGui.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            // РђРІС‚РѕРїСЂРѕРєСЂСѓС‚РєР° Р¶СѓСЂРЅР°Р»Р° РІРЅРёР·
            LogBox.TextChanged += (_, _) => LogBox.ScrollToEnd();
        }
    }
}
