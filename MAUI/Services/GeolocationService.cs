using BluForTracker.Shared;
using BluForTracker.Client.Shared.Services;
using Android.App;

namespace BluForTracker.Client.MAUI.Services;

public class GeolocationService : IGeolocationService, IDisposable
{
    private bool _isCheckingLocation = false;
    private CancellationTokenSource _cancelTokenSource;
    private Location? _currentLocation;
    public Task Init()
    {
        LocationLoop();
        return Task.CompletedTask;
    }

    PeriodicTimer? _mapLoadTimer;
    async Task LocationLoop()
    {

        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
        _mapLoadTimer ??= new(TimeSpan.FromMilliseconds(1000));
        while(await _mapLoadTimer.WaitForNextTickAsync())
        {
            try
            {
                if(!_isCheckingLocation)
                {
                    _isCheckingLocation = true;
                    _cancelTokenSource = new CancellationTokenSource();
                    _currentLocation = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                }
            } catch(Exception ex)
            {
                // Unable to get location
            } finally
            {
                _isCheckingLocation = false;
            }
        }
    }

    public async Task<Spike?> GetCurrentLocation()
    {
        await Task.CompletedTask;
        if(_currentLocation == null) return null;
        var result = new Spike
        {
            Latitude = _currentLocation.Latitude,
            Longitude = _currentLocation.Longitude,
            UpdatedOn = DateTime.UtcNow,
        };
        _currentLocation = null;
        return result;
    }

    public async Task<string?> Status()
    {
        await Task.CompletedTask;
        return "Not implemented";
    }

    public void Dispose()
    {
        if(_mapLoadTimer != null)
        {
            _mapLoadTimer.Dispose();
            _mapLoadTimer = null;
        }
    }
}
