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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7095")  // Your Blazor app URL -- TODO figure out 
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
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
app.UseCors("AllowBlazor");

app.MapHub<MarkerHub>(Routing.MarkerHub.Path);

// Minimal API
app.MapGet($"/api/CurrentVersion", () => typeof(Program).Assembly.GetName().Version?.ToString() ?? DateTime.UtcNow.Ticks.ToString());

app.Run();
