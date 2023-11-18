using System.Collections.Concurrent;

namespace BluForTracker.Shared;

public record Team {
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
