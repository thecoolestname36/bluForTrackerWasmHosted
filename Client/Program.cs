using BluForTracker.Client;
using BluForTracker.Client.Interops;
using BluForTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

if (!OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Options pattern
builder.Services.Configure<AppSettings>(opts =>
{
    opts.ApiHost = builder.Configuration["ApiHost"] ?? throw new Exception("Required appsetting 'ApiHost' is missing");
    opts.GoogleMapsApiKey = builder.Configuration["GoogleMapsApiKey"] ?? throw new Exception("Required appsetting 'GoogleMapsApiKey' is missing");
});
builder.Services.AddSingleton<GoogleMapsInterop>();   
builder.Services.AddSingleton<CurrentUserHandler>();
// TODO builder.Services.AddSingleton<MapHubConnection>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient<BluForTrackerApiService>((services, client) =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["ApiHost"]}/api/");
});

await builder.Build().RunAsync();

public class AppSettings
{
    public required string ApiHost { get; set; }
    public required string GoogleMapsApiKey { get; set; }
}
