using System.IO;
using ZapretGui.Models;

namespace ZapretGui.Services
{
    /// <summary>Р С›Р В±РЎвЂ°Р ВµР Вµ РЎРѓР С•РЎРѓРЎвЂљР С•РЎРЏР Р…Р С‘Р Вµ Р С—РЎР‚Р С‘Р В»Р С•Р В¶Р ВµР Р…Р С‘РЎРЏ: Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р в„–Р С”Р С‘, РЎРѓР С—Р С‘РЎРѓР С•Р С” Р С—РЎР‚Р С•РЎвЂћР С‘Р В»Р ВµР в„–, Р В°Р С”РЎвЂљР С‘Р Р†Р Р…РЎвЂ№Р в„– Р С—РЎР‚Р С•РЎвЂћР С‘Р В»РЎРЉ.</summary>
    public sealed class AppState
    {
        private readonly SettingsService _settingsService;
        private readonly ProfileService _profileService = new();
        private IReadOnlyList<ProfileInfo> _profiles = Array.Empty<ProfileInfo>();

        public AppState(SettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = settingsService.Load();

            if (string.IsNullOrWhiteSpace(Settings.ZapretPath))
            {
                var detected = AutoDetectFolder();
                if (detected != null)
                {
                    Settings.ZapretPath = detected;
                    Save();
                }
            }

            Reload();
        }

        public AppSettings Settings { get; }
        public IReadOnlyList<ProfileInfo> Profiles => _profiles;
        public ProfileInfo? ActiveProfile { get; private set; }

        public event EventHandler? ProfilesChanged;
        public event EventHandler? ActiveProfileChanged;

        public void Save() => _settingsService.Save(Settings);

        public void RefreshProfiles()
        {
            Reload();
            ProfilesChanged?.Invoke(this, EventArgs.Empty);
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SelectProfile(ProfileInfo profile)
        {
            Settings.SelectedProfile = profile.Name;
            Save();
            ActiveProfile = profile;
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetZapretPath(string path)
        {
            Settings.ZapretPath = path.Trim();
            Save();
            RefreshProfiles();
        }

        private void Reload()
        {
            _profiles = _profileService.Scan(Settings.ZapretPath);
            ActiveProfile = ProfileSelector.Select(
                _profiles, Settings.SelectedProfile, Settings.ZapretPath, Settings.AutoSelectProfile);
        }

        private static string? AutoDetectFolder()
        {
            var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var candidates = new[]
            {
                AppContext.BaseDirectory,
                @"C:\zapret",
                @"D:\zapret",
                Path.Combine(user, "zapret"),
                Path.Combine(user, "Downloads", "zapret")
            };

            return candidates.FirstOrDefault(c => ProfileSelector.CheckDependencies(c).Ok);
        }
    }
}

