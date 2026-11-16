using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ZapretGui.Models;
using ZapretGui.Services;

namespace ZapretGui.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly AppState _state;
        private AppSettings S => _state.Settings;

        private bool _dependencyOk;
        private string _dependencyStatus = string.Empty;

        public SettingsViewModel(AppState state)
        {
            _state = state;
            BrowseCommand = new RelayCommand(_ => Browse());
            UpdateDependencyStatus();
        }

        public ICommand BrowseCommand { get; }

        public string ZapretPath
        {
            get => S.ZapretPath;
            set
            {
                var path = (value ?? string.Empty).Trim();
                if (path == S.ZapretPath) return;
                _state.SetZapretPath(path);
                OnPropertyChanged();
                UpdateDependencyStatus();
            }
        }

        public string CustomArgs
        {
            get => S.CustomArgs;
            set { if (S.CustomArgs != value) { S.CustomArgs = value; Persist(); } }
        }

        public bool DependencyOk { get => _dependencyOk; private set => SetProperty(ref _dependencyOk, value); }
        public string DependencyStatus { get => _dependencyStatus; private set => SetProperty(ref _dependencyStatus, value); }
        public bool AutoSelectProfile
        {
            get => S.AutoSelectProfile;
            set { if (S.AutoSelectProfile == value) return; S.AutoSelectProfile = value; _state.Save(); OnPropertyChanged(); _state.RefreshProfiles(); }
        }
        public bool AutoStartZapret
        {
            get => S.AutoStartZapret;
            set { if (S.AutoStartZapret != value) { S.AutoStartZapret = value; Persist(); } }
        }
        public bool StopOnExit
        {
            get => S.StopOnExit;
            set { if (S.StopOnExit != value) { S.StopOnExit = value; Persist(); } }
        }
        public bool MinimizeToTray
        {
            get => S.MinimizeToTray;
            set { if (S.MinimizeToTray != value) { S.MinimizeToTray = value; Persist(); } }
        }
        public bool StartMinimized
        {
            get => S.StartMinimized;
            set { if (S.StartMinimized != value) { S.StartMinimized = value; Persist(); } }
        }
        public bool StartWithWindows
        {
            get => S.StartWithWindows;
            set { if (S.StartWithWindows == value) return; S.StartWithWindows = value; AutostartService.Set(value); Persist(); }
        }
        public bool IsLightTheme
        {
            get => string.Equals(S.Theme, "Light", StringComparison.OrdinalIgnoreCase);
            set { if (IsLightTheme == value) return; S.Theme = value ? "Light" : "Dark"; ThemeService.Apply(S.Theme); Persist(); }
        }

        private void Persist([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            _state.Save();
            OnPropertyChanged(name);
        }

        private void UpdateDependencyStatus()
        {
            var result = ProfileSelector.CheckDependencies(S.ZapretPath);
            DependencyOk = result.Ok;
            DependencyStatus = result.Message;
        }

        private void Browse()
        {
            var dialog = new OpenFolderDialog { Title = "Р вЂ™РЎвЂ№Р В±Р ВµРЎР‚Р С‘РЎвЂљР Вµ Р С—Р В°Р С—Р С”РЎС“ РЎРѓР С• РЎРѓР В±Р С•РЎР‚Р С”Р С•Р в„– Zapret" };
            if (Directory.Exists(S.ZapretPath)) dialog.InitialDirectory = S.ZapretPath;
            if (dialog.ShowDialog() == true) ZapretPath = dialog.FolderName;
        }
    }
}