using BluForTracker.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BluForTracker.Server;

public class MarkerHub : Hub
{
    public static readonly ConcurrentDictionary<string, Marker> Markers = new();
    public static readonly ConcurrentDictionary<string, InfoMarker> InfoMarkers = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.ReceiveConnectionId, Context.ConnectionId);
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.SyncMarkers, Markers.Values.ToList());
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.SyncInfoMarkers, InfoMarkers.Values.ToList());

        return;
        
        async Task RemoveOldClients()
        {
            foreach(var item in InfoMarkers)
            {
                if(item.Value == null || (DateTimeOffset.UtcNow - item.Value.CreatedOn).TotalSeconds > 3600)
                {
                    InfoMarkers.Remove(item.Key, out _);
                    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, item.Key);
                }
            }
            foreach(var item in Markers)
            {
                if(item.Value == null || (DateTimeOffset.UtcNow - item.Value.UpdatedOn).TotalSeconds > 3600)
                {
                    Markers.Remove(item.Key, out _);
                    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, item.Key);
                }
            }
        }
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
        Markers[marker.Id] = marker;
        await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveMarker, marker);
    }

    [HubMethodName(Routing.MarkerHub.Server.RemoveMarker)]
    public async Task RemoveMarker(string key) {
        Markers.TryRemove(key, out _);
        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, key);
        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, key);
    }

    [HubMethodName(Routing.MarkerHub.Server.Exit)]
    public async Task Exit()
    {
        InfoMarkers.TryRemove(Context.ConnectionId, out _);
        Markers.TryRemove(Context.ConnectionId, out _);
        await Clients.All.SendAsync(Routing.MarkerHub.Server.Exit, Context.ConnectionId);
    }

    [HubMethodName(Routing.MarkerHub.Server.BroadcastInfoMarker)]
    public async Task BroadcastInfoMarker(InfoMarker infoMarker) {
        infoMarker.CreatedOn = DateTimeOffset.UtcNow;
        infoMarker.Id = Context.ConnectionId;
        InfoMarkers[infoMarker.Id] = infoMarker;
        await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveInfoMarker, infoMarker);
    }

    [HubMethodName(Routing.MarkerHub.Server.RemoveInfoMarker)]
    public async Task RemoveInfoMarker(string key)
    {
        InfoMarkers.TryRemove(key, out _);
        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, key);
    }

    [HubMethodName(Routing.MarkerHub.Server.UpdateConnectionId)]
    public async Task UpdateConnectionId(string oldConnectionId, Team team, string labelColor) 
    {
        if(Markers.TryRemove(oldConnectionId, out var oldMarker)) {
            await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, oldConnectionId);
            await BroadcastMarker(oldMarker with {
                Id = Context.ConnectionId,
                Connected = true,
                Team = team,
                Color = labelColor
            });
        }
        if(InfoMarkers.TryRemove(oldConnectionId, out var oldInfoMarker)) {
            await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, oldConnectionId);
            await BroadcastInfoMarker(oldInfoMarker with {
                Id = Context.ConnectionId,
                Team = team,
                LabelColor = labelColor
            });
        }
    }
}
