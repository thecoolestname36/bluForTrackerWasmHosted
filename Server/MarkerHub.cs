using BluForTracker.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BluForTracker.Server;

public class MarkerHub : Hub
{
    public static readonly ConcurrentDictionary<string, Marker> Markers = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.ReceiveConnectionId, Context.ConnectionId);
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.ReceiveMarkers, Markers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        if(Markers.ContainsKey(Context.ConnectionId)) {
            Markers[Context.ConnectionId].Connected = false;
            await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveMarker, Markers[Context.ConnectionId]);
        }
    }

    [HubMethodName(Routing.MarkerHub.Server.BroadcastMarker)]
    public async Task BroadcastMarker(Marker marker)
    {
        marker.UpdatedOn = DateTimeOffset.UtcNow;
        marker.Id = Context.ConnectionId;
        Markers[Context.ConnectionId] = marker;
        foreach(var item in Markers)
        {
            if(item.Value == null || (DateTimeOffset.UtcNow - item.Value.UpdatedOn).TotalSeconds > 3600)
            {
                Markers.Remove(item.Key, out _);
                await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, item.Key);
            }
        }
        await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveMarker, marker);
    }

    [HubMethodName(Routing.MarkerHub.Server.RemoveMarker)]
    public async Task RemoveMarker(string key) {
        Markers.TryRemove(key, out _);
        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, key);
    }
}
