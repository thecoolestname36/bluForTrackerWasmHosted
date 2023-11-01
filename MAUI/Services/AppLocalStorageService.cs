using BluForTracker.Client.Shared.Services;

namespace BluForTracker.Client.MAUI;

public class AppLocalStorageService : IAppLocalStorageService
{
    public Task<string> GetItemAsStringAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetItemAsStringAsync(string key, string value)
    {
        throw new NotImplementedException();
    }
}
