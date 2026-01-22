using Microsoft.JSInterop;
using Microsoft.Maui.Storage;

namespace MoodJournal.Services;

public sealed class ThemeService
{
    private const string Key = "moodjournal_theme"; // light or dark
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
        await ApplyAsync();
    }

    public async Task ToggleAsync()
    {
        IsDark = !IsDark;
        Preferences.Default.Set(Key, IsDark ? "dark" : "light");
        await ApplyAsync();
    }

    private async Task ApplyAsync()
    {
        var theme = IsDark ? "dark" : "light";
        await _js.InvokeVoidAsync("moodJournalTheme.setTheme", theme);
    }
}