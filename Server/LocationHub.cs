using BluForTracker.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BluForTracker.Server;

public class LocationHub : Hub
{
    public static readonly ConcurrentDictionary<string, Marker> Markers = new();

    public async Task BroadcastLocation(Marker marker)
    {
        var now = DateTimeOffset.UtcNow;
        marker.UpdatedOn = now;
        marker.Id = Context.ConnectionId;

        Markers[marker.Id] = marker;
        foreach(var item in Markers)
        {
            if(item.Value == null || (now - item.Value.UpdatedOn).TotalSeconds > 3600)
            {
                Markers.Remove(item.Key, out _);
                continue;
            }
        }
        await Clients.All.SendAsync("NotifyLocation", new Dictionary<string, Marker>(Markers));
    }

    public void RemoveMarker(string key) => Markers.TryRemove(key, out _);
}
