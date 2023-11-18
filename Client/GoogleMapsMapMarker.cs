using BluForTracker.Shared;

namespace BluForTracker.Client.Shared;

public record GoogleMapsMapMarker {
    public required string ConnectionId { get; set; }
    public required string Label { get; set; }
    public required string Color { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}

public static partial class UserExtensions {
    public static GoogleMapsMapMarker? ToGoogleMapMarker(this User source) {
        if(string.IsNullOrEmpty(source.ConnectionId)) return null;
        return new GoogleMapsMapMarker
        {
            ConnectionId = source.ConnectionId,
            Label = source.Username ?? "",
            Color = source.Color ?? "#000000",
            Latitude = source.MapMarker?.Latitude ?? 0.0,
            Longitude = source.MapMarker?.Longitude ?? 0.0,
        };
    }
}
