using Blazored.LocalStorage;
using BluForTracker.Client.Blazor;
using BluForTracker.Client.Blazor.Services;
using BluForTracker.Client.Blazor.Services.JavaScript;
using BluForTracker.Client.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddSingleton<IAppLocalStorageService, AppLocalStorageService>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
builder.Services.AddHttpClient("BluForTracker.ServerAPI", client => {
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Add("X-BluForTracker", "dGhlY29vbGVzdG5hbWUzNg==");
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BluForTracker.ServerAPI"));

await builder.Build().RunAsync();
