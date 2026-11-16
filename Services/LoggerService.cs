using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace ZapretGui.Services
{
    public static class LoggerService
    {
        private static readonly BlockingCollection<string> _queue = new();
        private static readonly string _logPath;
        
        static LoggerService()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZapretGuiPreview");
            Directory.CreateDirectory(dir);
            _logPath = Path.Combine(dir, "app.log");
            
            var thread = new Thread(WriteLogs) { IsBackground = true };
            thread.Start();
        }

        public static void Log(string message)
        {
            _queue.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
        }

        private static void WriteLogs()
        {
            foreach (var msg in _queue.GetConsumingEnumerable())
            {
                try { File.AppendAllText(_logPath, msg + Environment.NewLine); } catch { }
            }
        }
    }
}
