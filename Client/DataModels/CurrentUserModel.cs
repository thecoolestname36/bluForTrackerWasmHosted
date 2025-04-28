using System.Drawing;
using BluForTracker.Shared;

namespace BluForTracker.Client.DataModels;

public record CurrentUserModel(string Label, string Color, Team? Team);

public static class UserStoreExtensions
{
    public static CurrentUserModel ToCurrentUserModel(this UserStore source)
    {
        var color = source.Color;
        try
        {
            // If it cant parse, default color to black
            ColorTranslator.FromHtml(color);
        }
        catch
        {
            color = "#000000";
        }
        return new CurrentUserModel(
            source.Label[..Math.Min(UserFormModel.LabelMaxChars, source.Label.Length)],
            color, 
            Team.None);
    }
}
