using System.Runtime.InteropServices;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace ZapretGui.Services
{
    public sealed class TrayService : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private readonly Forms.NotifyIcon _icon;
        private readonly Forms.ToolStripMenuItem _toggleItem;
        private readonly Drawing.Icon _runningIcon;
        private readonly Drawing.Icon _stoppedIcon;

        public TrayService(Action show, Action toggle, Action exit)
        {
            _runningIcon = CreateIcon(Drawing.Color.FromArgb(0, 230, 118));
            _stoppedIcon = CreateIcon(Drawing.Color.FromArgb(107, 114, 128));

            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Открыть Zapret GUI", null, (_, _) => show());
            _toggleItem = new Forms.ToolStripMenuItem("Запустить", null, (_, _) => toggle());
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add("Выход", null, (_, _) => exit());

            _icon = new Forms.NotifyIcon
            {
                Icon = _stoppedIcon,
                Text = "Zapret GUI — остановлено",
                ContextMenuStrip = menu,
                Visible = true
            };
            _icon.MouseDoubleClick += (_, e) =>
            {
                if (e.Button == Forms.MouseButtons.Left) show();
            };
        }

        public void SetRunning(bool running)
        {
            _icon.Icon = running ? _runningIcon : _stoppedIcon;
            _icon.Text = running ? "Zapret GUI — запущено" : "Zapret GUI — остановлено";
            _toggleItem.Text = running ? "Остановить" : "Запустить";
        }

        public void ShowBalloon(string title, string text)
            => _icon.ShowBalloonTip(2500, title, text, Forms.ToolTipIcon.Info);

        private static Drawing.Icon CreateIcon(Drawing.Color color)
        {
            using var bmp = new Drawing.Bitmap(32, 32);
            using (var g = Drawing.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Drawing.Color.Transparent);

                using var brush = new Drawing.SolidBrush(color);
                var shieldPoints = new Drawing.PointF[] {
                    new(16, 2), new(30, 8), new(30, 18), new(16, 30), new(2, 18), new(2, 8)
                };
                g.FillPolygon(brush, shieldPoints);

                using var darkBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(10, 10, 10));
                var boltPoints = new Drawing.PointF[] {
                    new(18, 6), new(10, 16), new(16, 16), new(14, 26), new(22, 14), new(16, 14)
                };
                g.FillPolygon(darkBrush, boltPoints);
            }

            IntPtr handle = bmp.GetHicon();
            try
            {
                using var temp = Drawing.Icon.FromHandle(handle);
                return (Drawing.Icon)temp.Clone();
            }
            finally
            {
                DestroyIcon(handle);
            }
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
            _runningIcon.Dispose();
            _stoppedIcon.Dispose();
        }
    }
}