using System.Diagnostics;
using System.IO;
using System.Text;
using ZapretGui.Models;

namespace ZapretGui.Services
{
    public sealed class ZapretManager : IDisposable
    {
        private Process? _process;
        public event Action<string>? OutputReceived;

        static ZapretManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public bool Start(ProfileInfo profile, string customArgs, out string error)
        {
            error = string.Empty;
            try
            {
                StopLauncher();
                var oem = Encoding.GetEncoding(866);
                var args = $"/c \"\"{profile.FullPath}\" {customArgs}\"";
                
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(profile.FullPath) ?? string.Empty,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = oem,
                    StandardErrorEncoding = oem
                };

                var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                process.OutputDataReceived += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
                process.Exited += (_, _) => OutputReceived?.Invoke("[cmd] Р С—РЎР‚Р С•РЎвЂћР С‘Р В»РЎРЉ Р С•РЎвЂљРЎР‚Р В°Р В±Р С•РЎвЂљР В°Р В», Р С•Р С”Р Р…Р С• Р В·Р В°Р С—РЎС“РЎРѓР С”Р В° Р В·Р В°Р С”РЎР‚РЎвЂ№РЎвЂљР С•");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.Close();

                _process = process;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public void StopLauncher()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                    _process.Kill(true);
            }
            catch { }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }

        public void Dispose() => StopLauncher();
    }
}