using System.IO;
using System.Text.Json;
using System.Windows;

namespace FiveW2H.App.UI.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }

    event EventHandler? ThemeChanged;

    void Initialize();

    void ApplyTheme(bool isDark);

    void Toggle();
}

public sealed class ThemeService : IThemeService
{
    private static readonly Uri DarkThemeUri = new("Resources/Themes/DarkTheme.xaml", UriKind.Relative);
    private static readonly Uri LightThemeUri = new("Resources/Themes/LightTheme.xaml", UriKind.Relative);

    private readonly string _settingsPath;

    public ThemeService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "5W2H-Management");
        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "ui-settings.json");
    }

    public bool IsDarkTheme { get; private set; } = true;

    public event EventHandler? ThemeChanged;

    public void Initialize()
    {
        var isDark = LoadSavedThemePreference();
        ApplyTheme(isDark);
    }

    public void ApplyTheme(bool isDark)
    {
        if (Application.Current is null)
        {
            IsDarkTheme = isDark;
            return;
        }

        var merged = Application.Current.Resources.MergedDictionaries;
        ResourceDictionary? currentTheme = null;

        foreach (var dictionary in merged)
        {
            var source = dictionary.Source?.OriginalString;
            if (source is not null &&
                (source.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                 source.Contains("LightTheme.xaml", StringComparison.OrdinalIgnoreCase)))
            {
                currentTheme = dictionary;
                break;
            }
        }

        var themeChanged = currentTheme is null || IsDarkTheme != isDark;

        if (currentTheme is not null)
        {
            merged.Remove(currentTheme);
        }

        merged.Insert(0, new ResourceDictionary
        {
            Source = isDark ? DarkThemeUri : LightThemeUri
        });

        IsDarkTheme = isDark;
        SaveThemePreference(isDark);

        if (themeChanged)
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Toggle() => ApplyTheme(!IsDarkTheme);

    private bool LoadSavedThemePreference()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return true;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<UiSettings>(json);
            return settings?.IsDarkTheme ?? true;
        }
        catch
        {
            return true;
        }
    }

    private void SaveThemePreference(bool isDark)
    {
        try
        {
            var settings = new UiSettings { IsDarkTheme = isDark };
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Preference persistence is best-effort.
        }
    }

    private sealed class UiSettings
    {
        public bool IsDarkTheme { get; set; } = true;
    }
}
