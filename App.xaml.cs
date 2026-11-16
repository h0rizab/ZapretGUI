using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ZapretGui.Services;
using ZapretGui.ViewModels;

namespace ZapretGui
{
    public partial class App : Application
    {
        private Mutex? _mutex;
        private TrayService? _tray;
        private MainWindow? _window;
        private bool _balloonShown;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private HwndSource? _source;

        public AppState State { get; private set; } = null!;
        public bool IsExiting { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool elevated = e.Args.Contains("--elevated");
            _mutex = new Mutex(false, @"Local\ZapretGuiPreview.SingleInstance");
            bool owned;
            try { owned = _mutex.WaitOne(elevated ? 5000 : 0); }
            catch (AbandonedMutexException) { owned = true; }

            if (!owned)
            {
                MessageBox.Show("Zapret GUI СѓР¶Рµ Р·Р°РїСѓС‰РµРЅ. РС‰РёС‚Рµ РёРєРѕРЅРєСѓ РІ СЃРёСЃС‚РµРјРЅРѕРј С‚СЂРµРµ.",
                    "Zapret GUI", MessageBoxButton.OK, MessageBoxImage.Information);
                _mutex.Dispose();
                _mutex = null;
                Shutdown();
                return;
            }

            State = new AppState(new SettingsService());
            ThemeService.Apply(State.Settings.Theme);

            var vm = new MainViewModel(State);
            _window = new MainWindow { DataContext = vm };
            _window.SourceInitialized += (_, _) => SetupHotkey();

            _tray = new TrayService(ShowMainWindow, vm.Dashboard.Toggle, () => ExitApplication());
            vm.Dashboard.RunningChanged += (_, running) => _tray.SetRunning(running);
            _tray.SetRunning(vm.Dashboard.IsRunning);

            if (!State.Settings.StartMinimized)
                _window.Show();

            vm.Dashboard.AutoStartIfNeeded();
        }

        private void SetupHotkey()
        {
            var helper = new WindowInteropHelper(_window!);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source?.AddHook(HwndHook);
            // Ctrl (2) + Alt (1) = 3, 'Z' = 0x5A
            RegisterHotKey(helper.Handle, HOTKEY_ID, 3, 0x5A);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID)
            {
                if (_window?.DataContext is MainViewModel vm)
                    vm.Dashboard.Toggle();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void ShowMainWindow()
        {
            if (_window == null) return;
            _window.Show();
            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;
            _window.Activate();
        }

        public void NotifyHiddenToTray()
        {
            if (_balloonShown) return;
            _balloonShown = true;
            _tray?.ShowBalloon("Zapret GUI", "РџСЂРёР»РѕР¶РµРЅРёРµ РїСЂРѕРґРѕР»Р¶Р°РµС‚ СЂР°Р±РѕС‚Р°С‚СЊ РІ С‚СЂРµРµ.\nР“РѕСЂСЏС‡Р°СЏ РєР»Р°РІРёС€Р°: Ctrl+Alt+Z");
        }

        public void ExitApplication(bool stopZapret = true)
        {
            if (IsExiting) return;
            IsExiting = true;

            if (stopZapret && State.Settings.StopOnExit)
                ProcessManager.KillWinws();

            if (_source != null)
            {
                UnregisterHotKey(new WindowInteropHelper(_window!).Handle, HOTKEY_ID);
                _source.RemoveHook(HwndHook);
                _source = null;
            }

            _tray?.Dispose();
            _tray = null;

            try { _mutex?.ReleaseMutex(); } catch { }
            _mutex?.Dispose();
            _mutex = null;

            Shutdown();
        }

        public void RestartAsAdministrator()
        {
            try
            {
                var exe = Environment.ProcessPath ?? string.Empty;
                var args = "--elevated";
                if (string.Equals(Path.GetFileNameWithoutExtension(exe), "dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    var dll = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
                    args = $"\"{dll}\" --elevated";
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas"
                });

                ExitApplication(stopZapret: false);
            }
            catch (Win32Exception) { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _tray?.Dispose();
            base.OnExit(e);
        }
    }
}

