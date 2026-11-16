using System.Diagnostics;
using System.Security.Principal;
using System.Threading;

namespace ZapretGui.Services
{
    public static class ProcessManager
    {
        public static bool IsWinwsRunning()
        {
            var processes = Process.GetProcessesByName("winws");
            try
            {
                return processes.Length > 0;
            }
            finally
            {
                foreach (var p in processes) p.Dispose();
            }
        }

        public static int KillWinws()
        {
            int killed = 0;
            foreach (var p in Process.GetProcessesByName("winws"))
            {
                try
                {
                    p.Kill(true);
                    p.WaitForExit(2000);
                    killed++;
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            if (killed > 0)
                Thread.Sleep(500);

            return killed;
        }

        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
