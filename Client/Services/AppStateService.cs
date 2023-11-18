using BluForTracker.Shared;

namespace BluForTracker.Client.Shared.Services;

public class AppStateService
{
    private User User = new();
    public User GetUser() => User;
}
