using System.IO;
namespace ZapretGui.ViewModels
{
    public sealed class ConnectivityItem : ViewModelBase
    {
        private string _state = "idle";      // idle | pending | ok | fail
        private string _statusText = "РІР‚вЂќ";

        public ConnectivityItem(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }
        public string Url { get; }

        public string State
        {
            get => _state;
            private set => SetProperty(ref _state, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public void SetPending()
        {
            State = "pending";
            StatusText = "Р С—РЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°РІР‚В¦";
        }

        public void SetResult(bool ok, string text)
        {
            State = ok ? "ok" : "fail";
            StatusText = text;
        }
    }
}


