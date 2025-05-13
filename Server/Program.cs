using BluForTracker.Client;
using BluForTracker.Client.Services;
using Microsoft.AspNetCore.ResponseCompression;
using BluForTracker.Shared;
using BluForTracker.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          new[] { "application/octet-stream" });
});

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
} else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();   
// }
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapHub<MarkerHub>(Routing.MarkerHub.Path);
app.MapFallbackToPage("/App");

// Minimal API
app.MapGet($"/{BluForTrackerApiService.BasePath}/{nameof(BluForTrackerApiService.CurrentVersion)}", () => typeof(App).Assembly.GetName().Version?.ToString() ?? DateTime.UtcNow.Ticks.ToString());

app.Run();
