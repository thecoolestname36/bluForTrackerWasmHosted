using BluForTracker.Client;
using BluForTracker.Client.Interops;
using BluForTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
if (OperatingSystem.IsBrowser())
{
    builder.Services.AddSingleton<GoogleMapsInterop>();   
}
else
{
    throw new Exception("Need a browser you dummy, this is a web app!");
}
builder.Services.AddSingleton<CurrentUserHandler>();
builder.Services.AddHttpClient<BluForTrackerApiService>(client =>
{
    client.BaseAddress = new Uri("https://blufortracker.sradzone.com/api/");
});

await builder.Build().RunAsync();
