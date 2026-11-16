using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using ZapretGui.Models;
using ZapretGui.Services;

namespace ZapretGui.ViewModels
{
    public sealed class ProfileItem : ViewModelBase
    {
        private bool _isSelected;

        public ProfileItem(ProfileInfo info, bool isSelected)
        {
            Info = info;
            _isSelected = isSelected;
        }

        public ProfileInfo Info { get; }
        public string Name => Info.Name;
        public string Description => Path.GetFileName(Info.FullPath);

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public sealed class ProfilesViewModel : ViewModelBase
    {
        private readonly AppState _state;
        private readonly ObservableCollection<ProfileItem> _items = new();
        private string _search = string.Empty;

        public ProfilesViewModel(AppState state)
        {
            _state = state;

            ProfilesView = CollectionViewSource.GetDefaultView(_items);
            ProfilesView.Filter = o => o is ProfileItem p
                && (string.IsNullOrWhiteSpace(_search)
                    || p.Name.Contains(_search, StringComparison.OrdinalIgnoreCase));

            SelectCommand = new RelayCommand(p =>
            {
                if (p is ProfileItem item) _state.SelectProfile(item.Info);
            });
            RefreshCommand = new RelayCommand(_ => _state.RefreshProfiles());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());

            _state.ProfilesChanged += (_, _) => Rebuild();
            _state.ActiveProfileChanged += (_, _) => UpdateSelection();
            Rebuild();
        }

        public ICollectionView ProfilesView { get; }
        public ICommand SelectCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public bool HasNoProfiles => _items.Count == 0;

        public string EmptyText => string.IsNullOrWhiteSpace(_state.Settings.ZapretPath)
            ? "Р СџР В°Р С—Р С”Р В° Zapret Р Р…Р Вµ РЎС“Р С”Р В°Р В·Р В°Р Р…Р В°. Р вЂ”Р В°Р Т‘Р В°Р в„–РЎвЂљР Вµ Р ВµРЎвЂ Р Р…Р В° Р Р†Р С”Р В»Р В°Р Т‘Р С”Р Вµ Р’В«Р СњР В°РЎРѓРЎвЂљРЎР‚Р С•Р в„–Р С”Р С‘Р’В»."
            : "Р вЂ™ Р С—Р В°Р С—Р С”Р Вµ Р Р…Р ВµРЎвЂљ BAT-РЎвЂћР В°Р в„–Р В»Р С•Р Р†. Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚РЎРЉРЎвЂљР Вµ Р С—РЎС“РЎвЂљРЎРЉ Р Р…Р В° Р Р†Р С”Р В»Р В°Р Т‘Р С”Р Вµ Р’В«Р СњР В°РЎРѓРЎвЂљРЎР‚Р С•Р в„–Р С”Р С‘Р’В».";

        public string SearchText
        {
            get => _search;
            set
            {
                if (SetProperty(ref _search, value))
                    ProfilesView.Refresh();
            }
        }

        private void Rebuild()
        {
            _items.Clear();
            foreach (var info in _state.Profiles)
                _items.Add(new ProfileItem(info, info == _state.ActiveProfile));

            OnPropertyChanged(nameof(HasNoProfiles));
            OnPropertyChanged(nameof(EmptyText));
        }

        private void UpdateSelection()
        {
            foreach (var item in _items)
                item.IsSelected = item.Info == _state.ActiveProfile;
        }

        private void OpenFolder()
        {
            var path = _state.Settings.ZapretPath;
            if (!Directory.Exists(path)) return;
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
            }
            catch
            {
                // Р С—РЎР‚Р С•Р Р†Р С•Р Т‘Р Р…Р С‘Р С” Р Р…Р ВµР Т‘Р С•РЎРѓРЎвЂљРЎС“Р С—Р ВµР Р…
            }
        }
    }
}

