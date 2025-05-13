namespace BluForTracker.Client.Services;

public class BluForTrackerApiService(HttpClient httpClient)
{
    public const string BasePath = "api";
    
    public Task<string> CurrentVersion() => httpClient.GetStringAsync(nameof(CurrentVersion));
}