using BluForTracker.Shared;

namespace BluForTracker.Client.Shared.Services;

public enum GeolocationStatus {
    Error = -1,
    NotInitialized = 0,
    Starting = 1,
    Started = 2,
    LocationSuccess = 3
}

public interface IGeolocationService
{
    Task Init();
    Task<Spike?> GetCurrentLocation();
    Task<string?> Status();
}
