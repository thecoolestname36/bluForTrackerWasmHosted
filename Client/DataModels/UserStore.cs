using BluForTracker.Shared;

namespace BluForTracker.Client.DataModels;

public record UserStore(string Label, string Color)
{
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