using System.Collections.Concurrent;

namespace BluForTracker.Shared;

public record Team {
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

public record TeamRoster : Team
{
    public List<UserInfo> UserInfos { get; set; } = new();
}
