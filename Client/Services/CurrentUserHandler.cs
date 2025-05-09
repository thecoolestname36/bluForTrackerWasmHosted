using BluForTracker.Client.DataModels;

namespace BluForTracker.Client.Services;

public class CurrentUserHandler : IAsyncDisposable
{
    public CurrentUserModel? CurrentUser { get; set; }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        CurrentUser = null;
    }
}