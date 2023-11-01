namespace BluForTracker.Client.Shared.Services;

public interface IAppLocalStorageService
{
    Task<string> GetItemAsStringAsync(string key);

    Task SetItemAsStringAsync(string key, string value);
}
