using BluForTracker.Client.MAUI;
using BluForTracker.Client.MAUI.Services;
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

        builder.Services.AddSingleton<IAppLocalStorageService, AppLocalStorageService>();
        builder.Services.AddSingleton<IAppLocalStorageService, AppLocalStorageService>();
        builder.Services.AddScoped<IGeolocationService, GeolocationService>();
        builder.Services.AddHttpClient("BluForTracker.ServerAPI", client => {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Add("X-BluForTracker", "dGhlY29vbGVzdG5hbWUzNg==");
        });
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BluForTracker.ServerAPI"));
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}