namespace BluForTracker.Shared;

public class Marker
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Title { get; set; }

    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;

    public string Color { get; set; } = "000000";
}
