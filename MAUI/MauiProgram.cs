using BluForTracker.Client.MAUI;
using BluForTracker.Client.MAUI.Services;
using BluForTracker.Client.Shared;
using BluForTracker.Client.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MAUI;

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
            });
        
        builder.Services.AddScoped<IAppLocalStorageService, AppLocalStorageService>();
        builder.Services.AddScoped<IGeolocationService, GeolocationService>();
        builder.Services.AddScoped<AppStateService>();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddSignalRHubConnection(new Uri("http://127.0.0.1:5137/", UriKind.Absolute));
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#else
        builder.Services.AddSignalRHubConnection(new Uri("https://blufortracker.sradzone.com/", UriKind.Absolute));
#endif

        return builder.Build();
    }
}