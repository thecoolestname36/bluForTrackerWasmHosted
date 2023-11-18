using BluForTracker.Shared;

namespace BluForTracker.Client.Shared;

public record GoogleMapsInfoMarker {
    public required string ConnectionId;
    public required string Username;
    public required string Color;
    public required InfoMarker InfoMarker;
}

public static partial class UserExtensions {
    public static GoogleMapsInfoMarker? ToGoogleMapsInfoMarker(this User source) {
        if(string.IsNullOrEmpty(source.ConnectionId) ||
            string.IsNullOrEmpty(source.Username) ||
            string.IsNullOrEmpty(source.Color) ||
            source.InfoMarker == null) return null;

        return new GoogleMapsInfoMarker {
            ConnectionId = source.ConnectionId,
            Username = source.Username,
            Color = source.Color,
            InfoMarker = source.InfoMarker,
        };
    }
}
