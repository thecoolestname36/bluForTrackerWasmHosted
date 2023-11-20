namespace BluForTracker.Shared;

public record User
{
    public string? ConnectionId { get; set; } = null;
    public string Username { get; set; } = "";
    public Guid TeamId { get; set; } = Guid.Empty;
    public bool IsActive { get; set; } = true;
    public string Color { get; set; } = string.Format("#{0:X6}", new Random().Next(0x1000000));
    public MapMarker? MapMarker { get; set; }
    public InfoMarker? InfoMarker { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;
    public bool IsValid() => !string.IsNullOrEmpty(Username);
}

public record UserInfo 
{
    public string? ConnectionId { get; set; } = null;
    public required string Username { get; set; }
    public required bool IsActive { get; set; }
    public required string Color { get; set; }
}
