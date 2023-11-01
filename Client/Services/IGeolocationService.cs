namespace BluForTracker.Client.Shared.Services;

public interface IGeolocationService
{
    Task Init();

    Task Log(string log);
}
