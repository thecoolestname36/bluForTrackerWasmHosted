using BluForTracker.Client.Shared.Services;
using Microsoft.JSInterop;
using System.Runtime.Versioning;

namespace BluForTracker.Client.Blazor.Services.JavaScript;

public partial class GeolocationService : IGeolocationService, IAsyncDisposable
{
    private readonly IJSRuntime? _jSRuntime;
    private IJSRuntime GetJSRuntime() => _jSRuntime ?? throw new NullReferenceException(nameof(JSRuntime));
    private IJSObjectReference? _geolocationServiceJS;
    private IJSObjectReference GetGeolocationServiceJS() => _geolocationServiceJS ?? throw new NullReferenceException($"{nameof(GeolocationService)}.");

    public GeolocationService(IJSRuntime jsRuntime)
    {
        _jSRuntime = jsRuntime;
    }

    public async Task Init()
    {
        var collocatedJs = await GetJSRuntime().InvokeAsync<IJSObjectReference>("import", "./Services/JavaScript/GeolocationService.razor.js");
        _geolocationServiceJS = await collocatedJs.InvokeAsync<IJSObjectReference>("CreateGeolocationService");
    }

    [SupportedOSPlatform("browser")]
    public async Task Log(string log)
    {
        await GetGeolocationServiceJS().InvokeVoidAsync("writeLine", log);
    }

    public ValueTask DisposeAsync()
    {
        return GetGeolocationServiceJS().DisposeAsync();
    }
}
