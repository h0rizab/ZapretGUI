using System.IO;
using ZapretGui.Models;

namespace ZapretGui.Services
{
    public sealed class ProfileService
    {
        public IReadOnlyList<ProfileInfo> Scan(string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return Array.Empty<ProfileInfo>();

            try
            {
                return Directory.EnumerateFiles(folder, "*.bat", SearchOption.TopDirectoryOnly)
                    .Select(f => new ProfileInfo(Path.GetFileNameWithoutExtension(f), f))
                    .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return Array.Empty<ProfileInfo>();
            }
        }
    }
}

