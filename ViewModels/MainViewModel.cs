using System.IO;
using System.Windows.Input;
using ZapretGui.Services;

namespace ZapretGui.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private object _currentView;

        public MainViewModel(AppState state)
        {
            Dashboard = new DashboardViewModel(state);
            Profiles = new ProfilesViewModel(state);
            Settings = new SettingsViewModel(state);

            _currentView = Dashboard;
            NavigateCommand = new RelayCommand(Navigate);
        }

        public DashboardViewModel Dashboard { get; }
        public ProfilesViewModel Profiles { get; }
        public SettingsViewModel Settings { get; }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand NavigateCommand { get; }

        private void Navigate(object? parameter)
        {
            CurrentView = (parameter as string) switch
            {
                "Profiles" => Profiles,
                "Settings" => Settings,
                _ => Dashboard
            };
        }
    }
}


