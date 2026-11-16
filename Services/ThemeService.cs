using System.IO;
using System.Windows;

namespace ZapretGui.Services
{
    public static class ThemeService
    {
        public static void Apply(string theme)
        {
            var name = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
            var dict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Themes/{name}.xaml", UriKind.Absolute)
            };

            var merged = Application.Current.Resources.MergedDictionaries;
            if (merged.Count > 0) merged[0] = dict;
            else merged.Insert(0, dict);
        }
    }
}

