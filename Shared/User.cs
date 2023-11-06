namespace BluForTracker.Shared;

public class User
{
    public string Id { get; set; } = "unk";
    public string? Label { get; set; }
    public Team Team { get; set; }
    public string Color { get; set; } = string.Format("#{0:X6}", new Random().Next(0x1000000));
    private Marker? _marker = null;
    public Marker? Marker
    {
        get => new Marker
        {
            Id = Id,
            Label = Label ?? "unk",
            Connected = true,
            Color = Color,
            Spike = _marker?.Spike ?? new Spike()
        }; set => _marker = value;
    }
    public InfoMarker? InfoMarker { get; set; }
}
