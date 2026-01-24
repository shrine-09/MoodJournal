using Microsoft.JSInterop;
using Microsoft.Maui.Storage;

namespace MoodJournal.Services;

public sealed class ThemeService
{
    private const string Key = "mj_theme";
    private readonly IJSRuntime _js;

    public bool IsDark { get; private set; }

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitAsync()
    {
        var saved = Preferences.Default.Get(Key, "light");
        IsDark = saved.Equals("dark", StringComparison.OrdinalIgnoreCase);

        //Apply and keep localStorage in sync via JS
        await _js.InvokeVoidAsync("moodJournalTheme.setTheme", IsDark ? "dark" : "light");
    }

    public async Task ToggleAsync()
    {
        IsDark = !IsDark;

        var theme = IsDark ? "dark" : "light";
        Preferences.Default.Set(Key, theme);

        await _js.InvokeVoidAsync("moodJournalTheme.setTheme", theme);
    }

    public async Task SetAsync(bool dark)
    {
        IsDark = dark;

        var theme = IsDark ? "dark" : "light";
        Preferences.Default.Set(Key, theme);

        await _js.InvokeVoidAsync("moodJournalTheme.setTheme", theme);
    }
}