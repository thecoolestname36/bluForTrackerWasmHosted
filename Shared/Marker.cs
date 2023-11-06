using System.ComponentModel.DataAnnotations;

namespace BluForTracker.Shared;

public record Marker
{
    public string Id { get; set; } = "unk";
    public string Label { get; set; } = "";
    public bool Connected { get; set; } = true;
    public string Color { get; set; } = "#000000";
    public Spike Spike { get; set; } = new();
}
