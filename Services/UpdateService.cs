using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZapretGui.Services
{
    public static class UpdateService
    {
        public static async Task<string?> CheckLatestVersionAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ZapretGui/1.0");
                
                // Р ВРЎРѓР С—Р С•Р В»РЎРЉР В·РЎС“Р ВµР С API GitHub Р Т‘Р В»РЎРЏ Р С—Р С•Р В»РЎС“РЎвЂЎР ВµР Р…Р С‘РЎРЏ Р С—Р С•РЎРѓР В»Р ВµР Т‘Р Р…Р ВµР С–Р С• РЎР‚Р ВµР В»Р С‘Р В·Р В° (Р С—РЎР‚Р С‘Р СР ВµРЎР‚ Р Р…Р В° Р В±Р В°Р В·Р Вµ GoodbyeDPI/Zapret)
                var response = await client.GetAsync("https://api.github.com/repos/ValdikSS/GoodbyeDPI/releases/latest");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tag_name", out var tag))
                {
                    return tag.GetString();
                }
            }
            catch { }
            return null;
        }
    }
}