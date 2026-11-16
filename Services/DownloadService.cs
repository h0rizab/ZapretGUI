using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZapretGui.Services
{
    public static class DownloadService
    {
        public static async Task<bool> DownloadAndExtractZapretAsync(string destFolder, Action<string> log)
        {
            try
            {
                Directory.CreateDirectory(destFolder);
                var zipPath = Path.Combine(destFolder, "zapret.zip");
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ZapretGui/1.0");

                log("Р вЂ”Р В°Р С—РЎР‚Р С•РЎРѓ Р С” GitHub API...");
                var json = await client.GetStringAsync("https://api.github.com/repos/ValdikSS/GoodbyeDPI/releases/latest");
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                
                string? downloadUrl = null;
                if (doc.RootElement.TryGetProperty("assets", out var assets))
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        var name = asset.GetProperty("name").GetString();
                        if (name != null && name.Contains("win64") && name.EndsWith(".zip"))
                        {
                            downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    log("Р СњР Вµ РЎС“Р Т‘Р В°Р В»Р С•РЎРѓРЎРЉ Р Р…Р В°Р в„–РЎвЂљР С‘ РЎРѓРЎРѓРЎвЂ№Р В»Р С”РЎС“ Р Р…Р В° РЎРѓР С”Р В°РЎвЂЎР С‘Р Р†Р В°Р Р…Р С‘Р Вµ.");
                    return false;
                }

                log("Р РЋР С”Р В°РЎвЂЎР С‘Р Р†Р В°Р Р…Р С‘Р Вµ Р В°РЎР‚РЎвЂ¦Р С‘Р Р†Р В°...");
                var data = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(zipPath, data);

                log("Р В Р В°РЎРѓР С—Р В°Р С”Р С•Р Р†Р С”Р В°...");
                ZipFile.ExtractToDirectory(zipPath, destFolder, true);
                File.Delete(zipPath);
                
                log("Р Р€РЎРѓРЎвЂљР В°Р Р…Р С•Р Р†Р С”Р В° Р В·Р В°Р Р†Р ВµРЎР‚РЎв‚¬Р ВµР Р…Р В°!");
                return true;
            }
            catch (Exception ex)
            {
                log($"Р С›РЎв‚¬Р С‘Р В±Р С”Р В° РЎС“РЎРѓРЎвЂљР В°Р Р…Р С•Р Р†Р С”Р С‘: {ex.Message}");
                return false;
            }
        }
    }
}