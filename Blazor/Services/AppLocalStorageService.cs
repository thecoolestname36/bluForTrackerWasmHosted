using Blazored.LocalStorage;
using BluForTracker.Client.Shared.Services;

namespace BluForTracker.Client.Blazor.Services;

public class AppLocalStorageService : IAppLocalStorageService
{
    private readonly ILocalStorageService LocalStorage;
    public AppLocalStorageService(ILocalStorageService localStorage) {
        LocalStorage = localStorage;
    }

    public Task<string> GetItemAsStringAsync(string key) => LocalStorage.GetItemAsStringAsync(key).AsTask();

    public Task SetItemAsStringAsync(string key, string value) => LocalStorage.SetItemAsStringAsync(key, value).AsTask();
}
