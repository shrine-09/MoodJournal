using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MoodJournal.Services;

namespace MoodJournal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddScoped<ThemeService>();
        builder.Services.AddSingleton<JournalRepository>(); //for local SQLite repo
        builder.Services.AddSingleton<PinService>(); //pin for the lock
        builder.Services.AddSingleton<AppLockService>(); //system lock 
        builder.Services.AddSingleton<PdfExportService>(); //pdf esport service
        
        return builder.Build();
    }
}