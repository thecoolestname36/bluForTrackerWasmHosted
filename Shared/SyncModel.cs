namespace BluForTracker.Shared;

public class SyncModel
{
    public required string ConnectionId { get; set; }
    public required List<string> Teams { get; set; }
    public required List<Spike> Spikes { get; set; }
    public required List<InfoMarker> InfoMarkers { get; set; }
}
