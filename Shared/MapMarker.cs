namespace BluForTracker.Shared;

public record MapMarker
{
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;
}