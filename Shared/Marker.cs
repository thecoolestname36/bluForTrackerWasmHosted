namespace BluForTracker.Shared;

public class Marker
{
    public required string Id { get; set; }
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Color { get; set; } = "000000";
}
