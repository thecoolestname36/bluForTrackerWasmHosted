namespace BluForTracker.Shared;

public record Spike
{
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
}

public record SpikeBroadcast : Spike {
    public required string Id { get; init; }
}