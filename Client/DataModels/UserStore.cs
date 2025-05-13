using BluForTracker.Shared;

namespace BluForTracker.Client.DataModels;

public record UserStore(string Label, string Color)
{
    public const string StorageKey = "user_store";
    public UserFormModel ToUserFormModel() => new UserFormModel()
    {
        Label = Label,
        Team = Team.None
    };
}

public static class UserFormModelExtensions
{
    public static UserStore ToUserStore(this UserFormModel source) => new(source.Label, source.Color);
}