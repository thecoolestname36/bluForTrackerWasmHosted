namespace BluForTracker.Shared;

public record Marker
{
    public string Id { get; set; } = "unk";
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Color { get; set; } = "000000";
}
