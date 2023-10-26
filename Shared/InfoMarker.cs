namespace BluForTracker.Shared;

public record InfoMarker
{
    public string Id { get; set; } = "unk";
    public Team Team { get; set; }
    public string Label { get; set; } = "";
    public string LabelColor { get; set; } = "#000000";
    public string Message { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.MinValue;
}
