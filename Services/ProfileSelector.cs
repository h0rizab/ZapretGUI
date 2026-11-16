using System.IO;
using ZapretGui.Models;

namespace ZapretGui.Services
{
    public static class ProfileSelector
    {
        public static ProfileInfo? Select(
            IReadOnlyList<ProfileInfo> profiles, string? savedName, string? folder, bool autoSelect)
        {
            if (profiles.Count == 0) return null;

            // 1. РЎРѕС…СЂР°РЅС‘РЅРЅС‹Р№ РїСЂРѕС„РёР»СЊ
            if (!string.IsNullOrWhiteSpace(savedName))
            {
                var saved = profiles.FirstOrDefault(
                    p => string.Equals(p.Name, savedName, StringComparison.OrdinalIgnoreCase));
                if (saved != null) return saved;
            }

            if (!autoSelect) return null;

            // 2. Р—Р°РІРёСЃРёРјРѕСЃС‚Рё (winws.exe, WinDivert64.sys)
            if (!CheckDependencies(folder).Ok) return null;

            // 3. РњРёРЅРёРјР°Р»СЊРЅР°СЏ РґР»РёРЅР° РёРјРµРЅРё
            return profiles
                .OrderBy(p => p.Name.Length)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .First();
        }

        public static DependencyResult CheckDependencies(string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return new DependencyResult(false, "РџР°РїРєР° РЅРµ РЅР°Р№РґРµРЅР°");

            var missing = new List<string>();
            if (!FileExists(folder, "winws.exe")) missing.Add("winws.exe");
            if (!FileExists(folder, "WinDivert64.sys")) missing.Add("WinDivert64.sys");

            return missing.Count == 0
                ? new DependencyResult(true, "winws.exe Рё WinDivert64.sys РЅР°Р№РґРµРЅС‹")
                : new DependencyResult(false, "РќРµ РЅР°Р№РґРµРЅРѕ: " + string.Join(", ", missing));
        }

        private static bool FileExists(string folder, string fileName)
        {
            try
            {
                var options = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    MaxRecursionDepth = 3,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                };
                return Directory.EnumerateFiles(folder, fileName, options).Any();
            }
            catch
            {
                return false;
            }
        }
    }
}
