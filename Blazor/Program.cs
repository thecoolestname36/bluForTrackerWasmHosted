using Blazored.LocalStorage;
using BluForTracker.Client.Blazor;
using BluForTracker.Client.Blazor.Services;
using BluForTracker.Client.Shared;
using BluForTracker.Client.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var endpoint = new Uri(builder.Configuration["Endpoint"] ?? throw new IndexOutOfRangeException("Required parameter 'Endpoint' not found."), UriKind.Absolute);
builder.Services.AddSignalRHubConnection(endpoint);
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddScoped<IAppLocalStorageService, AppLocalStorageService>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
builder.Services.AddScoped<AppStateService>();

await builder.Build().RunAsync();
