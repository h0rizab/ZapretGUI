using System.IO;
using Microsoft.Win32;

namespace ZapretGui.Services
{
    public static class AutostartService
    {
        private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "ZapretGuiPreview";

        public static void Set(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(KeyPath, true);
                if (key == null) return;

                if (enable)
                {
                    var exe = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exe))
                        key.SetValue(ValueName, $"\"{exe}\"");
                }
                else
                {
                    key.DeleteValue(ValueName, false);
                }
            }
            catch
            {
                // Р Р…Р ВµРЎвЂљ Р Т‘Р С•РЎРѓРЎвЂљРЎС“Р С—Р В° Р С” РЎР‚Р ВµР ВµРЎРѓРЎвЂљРЎР‚РЎС“ РІР‚вЂќ Р С—РЎР‚Р С•Р С—РЎС“РЎРѓР С”Р В°Р ВµР С
            }
        }
    }
}

