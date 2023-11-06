namespace BluForTracker.Shared;

public record InfoMarker
{
    public string Id { get; set; } = "unk";
    public string Label { get; set; } = "";
    public string LabelColor { get; set; } = "#000000";
    public string Message { get; set; } = "";
    public Spike Spike { get; set; } = new Spike();
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.MinValue;
}
