using System.ComponentModel.DataAnnotations;

namespace BluForTracker.Shared;

public record Marker
{
    public string Id { get; set; } = "unk";
    public string Label { get; set; } = "";
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool Connected { get; set; } = true;
    public string Color { get; set; } = "000000";
}
