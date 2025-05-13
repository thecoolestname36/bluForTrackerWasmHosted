using Blazored.LocalStorage;
using BluForTracker.Client;
using BluForTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddSingleton<CurrentUserHandler>();
builder.Services.AddHttpClient<BluForTrackerApiService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api/");
});

await builder.Build().RunAsync();
