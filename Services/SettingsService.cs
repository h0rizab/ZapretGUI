using System.IO;
using System.Text.Json;
using ZapretGui.Models;

namespace ZapretGui.Services
{
    public sealed class SettingsService
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
        private readonly string _path;

        public SettingsService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ZapretGuiPreview");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "settings.json");
        }

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), Options);
                    if (loaded != null) return loaded;
                }
            }
            catch
            {
                // Р С—Р С•Р Р†РЎР‚Р ВµР В¶Р Т‘РЎвЂР Р…Р Р…РЎвЂ№Р в„– РЎвЂћР В°Р в„–Р В» Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” РІР‚вЂќ Р С‘РЎРѓР С—Р С•Р В»РЎРЉР В·РЎС“Р ВµР С Р В·Р Р…Р В°РЎвЂЎР ВµР Р…Р С‘РЎРЏ Р С—Р С• РЎС“Р СР С•Р В»РЎвЂЎР В°Р Р…Р С‘РЎР‹
            }
            return new AppSettings();
        }

        public void Save(AppSettings settings)
        {
            try
            {
                File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
            }
            catch
            {
                // Р Р…Р Вµ Р С”РЎР‚Р С‘РЎвЂљР С‘РЎвЂЎР Р…Р С•: Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р в„–Р С”Р С‘ Р С—РЎР‚Р С•РЎРѓРЎвЂљР С• Р Р…Р Вµ РЎРѓР С•РЎвЂ¦РЎР‚Р В°Р Р…РЎРЏРЎвЂљРЎРѓРЎРЏ
            }
        }
    }
}

