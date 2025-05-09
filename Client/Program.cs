using Blazored.LocalStorage;
using BluForTracker.Client;
using BluForTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddHttpClient("BluForTracker.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BluForTracker.ServerAPI"));
builder.Services.AddSingleton<CurrentUserHandler>();

await builder.Build().RunAsync();
