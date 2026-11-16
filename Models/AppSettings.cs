using System.IO;
namespace ZapretGui.Models
{
    public class AppSettings
    {
        public string ZapretPath { get; set; } = string.Empty;
        public string? SelectedProfile { get; set; }
        public bool AutoSelectProfile { get; set; } = true;
        public bool AutoStartZapret { get; set; }
        public bool StopOnExit { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public bool StartMinimized { get; set; }
        public bool StartWithWindows { get; set; }
        public string Theme { get; set; } = "Dark";
        public string CustomArgs { get; set; } = string.Empty;
    }
}