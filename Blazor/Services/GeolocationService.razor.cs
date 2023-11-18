using BluForTracker.Client.Shared.Services;
using BluForTracker.Shared;
using Microsoft.JSInterop;

namespace BluForTracker.Client.Blazor.Services;

public partial class GeolocationService : IGeolocationService, IAsyncDisposable
{
    private MapMarker? _currentLocation;
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
        var collocatedJs = await GetJSRuntime().InvokeAsync<IJSObjectReference>("import", "./Services/GeolocationService.razor.js");
        _geolocationServiceJS = await collocatedJs.InvokeAsync<IJSObjectReference>("CreateGeolocationService", DotNetObjectReference.Create(this));
        await _geolocationServiceJS.InvokeVoidAsync("watchPosition");
    }

    public async Task<MapMarker?> GetCurrentLocation()
    {
        await Task.CompletedTask;
        if (_currentLocation == null) return null;
        return new MapMarker
        {
            Latitude = _currentLocation.Latitude,
            Longitude = _currentLocation.Longitude,
            Timestamp = _currentLocation.Timestamp,
        };
    }

    [JSInvokable]
    public void UpdateCurrentPosition(double lat, double lng) => _currentLocation = new MapMarker
    {
        Latitude = lat,
        Longitude = lng,
        Timestamp = DateTimeOffset.UtcNow,
    };

    [JSInvokable]
    public void UpdateCurrentPositionError() => Console.WriteLine("GeolocationService UpdateCurrentPositionError");

    public async ValueTask DisposeAsync()
    {
        if (_geolocationServiceJS != null)
        {
            await _geolocationServiceJS.InvokeVoidAsync("dispose");
        }
        await GetGeolocationServiceJS().DisposeAsync().AsTask();
    }

    public async Task<string?> Status() {
        if(_geolocationServiceJS != null) {
            return (await _geolocationServiceJS.InvokeAsync<GeolocationStatus>("getStatus")).ToString();
        }
        return null;
    }
}
