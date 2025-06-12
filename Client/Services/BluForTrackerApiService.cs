namespace BluForTracker.Client.Services;

public class BluForTrackerApiService(HttpClient httpClient)
{
    public Task<string> CurrentVersion() => httpClient.GetStringAsync(nameof(CurrentVersion));
}