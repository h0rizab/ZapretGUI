using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ZapretGui.Services;

namespace ZapretGui.ViewModels
{
    public sealed class DashboardViewModel : ViewModelBase
    {
        private const int MaxLogLength = 30000;
        private static readonly HttpClient Http = CreateHttpClient();
        private readonly AppState _state;
        private readonly ZapretManager _manager = new();
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _pingTimer;
        private readonly StringBuilder _logBuilder = new();

        private bool _isRunning;
        private bool _isBusy;
        private bool _isChecking;
        private string _log = string.Empty;
        private int _pingMs;
        private string _updateStatus = "Проверить обновления";

        public DashboardViewModel(AppState state)
        {
            _state = state;

            Targets = new ObservableCollection<ConnectivityItem>
            {
                new("YouTube", "https://www.youtube.com"),
                new("Discord", "https://discord.com"),
                new("Telegram", "https://web.telegram.org"),
                new("Google", "https://www.google.com")
            };

            StartCommand = new RelayCommand(_ => _ = StartAsync(), _ => CanStart);
            StopCommand = new RelayCommand(_ => _ = StopAsync(), _ => CanStop);
            ClearLogCommand = new RelayCommand(_ => { _logBuilder.Clear(); Log = string.Empty; });
            CopyLogCommand = new RelayCommand(_ => CopyLog());
            CheckCommand = new RelayCommand(_ => _ = CheckAsync(), _ => !IsChecking);
            RestartAsAdminCommand = new RelayCommand(_ => ((App)Application.Current).RestartAsAdministrator());
            CheckUpdatesCommand = new RelayCommand(async _ => await CheckUpdatesAsync());
            DownloadZapretCommand = new RelayCommand(async _ => await DownloadZapretAsync(), _ => !IsBusy);

            IsAdmin = ProcessManager.IsAdministrator();

            _manager.OutputReceived += line =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Append(line)), DispatcherPriority.Background);

            _state.ActiveProfileChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(ProfileName));
                CommandManager.InvalidateRequerySuggested();
            };

            _isRunning = ProcessManager.IsWinwsRunning();

            _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += async (_, _) => 
            { 
                if (!_isBusy) 
                {
                    bool isRunningNow = ProcessManager.IsWinwsRunning();
                    if (_isRunning && !isRunningNow && _state.Settings.AutoStartZapret)
                    {
                        Append("Обнаружено падение winws.exe. Перезапуск (Watchdog)...");
                        await StartAsync();
                    }
                    else SetRunning(isRunningNow); 
                }
            };
            _timer.Start();

            _pingTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(3) };
            _pingTimer.Tick += async (_, _) => await UpdatePingAsync();
            _pingTimer.Start();

            Append("Готово. Выберите профиль и нажмите «Запустить» (или Ctrl+Alt+Z).");
            if (!IsAdmin)
                Append("Внимание: приложение запущено без прав администратора — WinDivert может не загрузиться.");
        }

        public ObservableCollection<ConnectivityItem> Targets { get; }
        public event EventHandler<bool>? RunningChanged;

        public bool IsAdmin { get; }
        public bool IsNotAdmin => !IsAdmin;
        public bool IsRunning => _isRunning;
        public bool IsBusy => _isBusy;
        public string StatusText => _isBusy ? "Подождите…" : (_isRunning ? "Запущено" : "Остановлено");
        public string ProfileName => _state.ActiveProfile?.Name ?? "не выбран";
        public bool IsChecking { get => _isChecking; private set => SetProperty(ref _isChecking, value); }
        public string Log { get => _log; set => SetProperty(ref _log, value); }
        public int PingMs { get => _pingMs; private set => SetProperty(ref _pingMs, value); }
        public string UpdateStatus { get => _updateStatus; private set => SetProperty(ref _updateStatus, value); }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand CopyLogCommand { get; }
        public ICommand CheckCommand { get; }
        public ICommand RestartAsAdminCommand { get; }
        public ICommand CheckUpdatesCommand { get; }
        public ICommand DownloadZapretCommand { get; }

        private bool CanStart => !_isRunning && !_isBusy && _state.ActiveProfile != null;
        private bool CanStop => _isRunning && !_isBusy;

        public void Toggle()
        {
            if (_state.ActiveProfile == null) { Append("Профиль не выбран."); return; }
            if (CanStop) _ = StopAsync();
            else if (CanStart) _ = StartAsync();
        }

        public void AutoStartIfNeeded()
        {
            if (_state.Settings.AutoStartZapret && CanStart) _ = StartAsync();
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            OnPropertyChanged(nameof(StatusText));
            CommandManager.InvalidateRequerySuggested();
        }

        private void SetRunning(bool running)
        {
            if (_isRunning == running) return;
            _isRunning = running;
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(StatusText));
            RunningChanged?.Invoke(this, running);
            Append(running ? "winws.exe запущен" : "winws.exe остановлен");
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task StartAsync()
        {
            var profile = _state.ActiveProfile;
            if (profile == null) return;

            var deps = ProfileSelector.CheckDependencies(_state.Settings.ZapretPath);
            if (!deps.Ok) { Append("Запуск невозможен. " + deps.Message); return; }

            SetBusy(true);
            try
            {
                Append($"Запуск профиля «{profile.Name}»…");
                if (!_manager.Start(profile, _state.Settings.CustomArgs, out var error))
                {
                    Append("Ошибка запуска: " + error);
                    return;
                }

                await Task.Delay(1500);
                SetRunning(ProcessManager.IsWinwsRunning());
                if (!_isRunning) Append("winws.exe не обнаружен. Проверьте права администратора.");
            }
            finally { SetBusy(false); }
        }

        private async Task StopAsync()
        {
            SetBusy(true);
            try
            {
                Append("Остановка…");
                _manager.StopLauncher();
                await Task.Run(ProcessManager.KillWinws);
                await Task.Delay(400);
                SetRunning(ProcessManager.IsWinwsRunning());
            }
            finally { SetBusy(false); }
        }

        private async Task CheckAsync()
        {
            IsChecking = true;
            foreach (var t in Targets) t.SetPending();
            try { await Task.WhenAll(Targets.Select(CheckOneAsync)); }
            finally { IsChecking = false; }
        }

        private static async Task CheckOneAsync(ConnectivityItem item)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, item.Url);
                using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                sw.Stop();
                item.SetResult(true, $"{sw.ElapsedMilliseconds} мс");
            }
            catch { item.SetResult(false, "недоступен"); }
        }

        private async Task UpdatePingAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("1.1.1.1", 2000);
                PingMs = reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : -1;
            }
            catch { PingMs = -1; }
        }

        private async Task CheckUpdatesAsync()
        {
            UpdateStatus = "Проверка...";
            var version = await UpdateService.CheckLatestVersionAsync();
            UpdateStatus = version != null ? $"Доступна: {version}" : "Обновлений нет";
        }

        private async Task DownloadZapretAsync()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZapretGuiPreview", "Zapret");
            SetBusy(true);
            try
            {
                bool ok = await DownloadService.DownloadAndExtractZapretAsync(folder, Append);
                if (ok)
                {
                    _state.SetZapretPath(folder);
                    Append($"Zapret установлен в {folder}");
                }
            }
            finally { SetBusy(false); }
        }

        private void CopyLog()
        {
            try { if (!string.IsNullOrEmpty(Log)) Clipboard.SetText(Log); } catch { }
        }

        private void Append(string line)
        {
            _logBuilder.AppendLine($"{DateTime.Now:HH:mm:ss}  {line}");
            LoggerService.Log(line);
            if (_logBuilder.Length > MaxLogLength)
            {
                var cut = _logBuilder.ToString().IndexOf('\n', _logBuilder.Length - (MaxLogLength / 2));
                if (cut > 0) _logBuilder.Remove(0, cut + 1);
            }
            Log = _logBuilder.ToString();
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 ZapretGui");
            return client;
        }
    }
}