using BluForTracker.Shared;
using Constants = BluForTracker.Shared.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BluForTracker.Server;

public class MarkerHub : Hub
{
    public static readonly ConcurrentDictionary<string, (ConcurrentDictionary<string, Marker> Markers, ConcurrentDictionary<string, InfoMarker> InfoMarkers)> Teams = new();
    public static readonly ConcurrentDictionary<string, Marker> Markers = new();
    public static readonly ConcurrentDictionary<string, InfoMarker> InfoMarkers = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        var httpCtx = Context.GetHttpContext();
        var secret = httpCtx?.Request.Query[Constants.Secret.Key];
        if(!secret.HasValue || secret.Value != Constants.Secret.Value) {
            Context.Abort();
            return;
        }

        //foreach(var item in InfoMarkers)
        //{
        //    if(item.Value == null || (DateTimeOffset.UtcNow - item.Value.CreatedOn).TotalSeconds > 3600)
        //    {
        //        InfoMarkers.Remove(item.Key, out _);
        //        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, item.Key);
        //    }
        //}
        //foreach(var item in Markers)
        //{
        //    if(item.Value == null || (DateTimeOffset.UtcNow - item.Value.UpdatedOn).TotalSeconds > 3600)
        //    {
        //        Markers.Remove(item.Key, out _);
        //        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, item.Key);
        //    }
        //}
        //await Clients.Caller.SendAsync(Routing.MarkerHub.Client.ReceiveConnectionId, Context.ConnectionId);
        //await Clients.Caller.SendAsync(Routing.MarkerHub.Client.SyncMarkers, Markers.Values.ToList());
        //await Clients.Caller.SendAsync(Routing.MarkerHub.Client.SyncInfoMarkers, InfoMarkers.Values.ToList());
        await Clients.Caller.SendAsync(Routing.MarkerHub.Client.ReceiveSync, new SyncModel {
            ConnectionId = Context.ConnectionId,
            Teams = Teams.Keys.ToList(),
            Spikes = Markers.Values.Select(s => s.Spike).ToList(),
            InfoMarkers = InfoMarkers.Values.ToList(),
        });
    }

    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    await base.OnDisconnectedAsync(exception);
    //    if(Markers.ContainsKey(Context.ConnectionId)) {
    //        Markers[Context.ConnectionId].Connected = false;
    //        await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveMarker, Markers[Context.ConnectionId]);
    //    }
    //}

    //[HubMethodName(Routing.MarkerHub.Server.BroadcastMarker)]
    //public async Task BroadcastMarker(Marker marker)
    //{
    //    marker.UpdatedOn = DateTimeOffset.UtcNow;
    //    marker.Id = Context.ConnectionId;
    //    Markers[marker.Id] = marker;
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveMarker, marker);
    //}

    [HubMethodName(Routing.MarkerHub.Server.BroadcastSpike)]
    public async Task BroadcastSpike(Spike spike)
    {
        spike.UpdatedOn = DateTime.UtcNow;
        Markers.AddOrUpdate(Context.ConnectionId, new Marker
        {
            Spike = spike
        }, (key, oldValue) => {
            
            oldValue.Spike.Longitude = spike.Longitude;
            oldValue.Spike.Latitude = spike.Latitude;
            oldValue.Spike.UpdatedOn = DateTime.UtcNow;
            return oldValue;
        });
        await Clients.All.SendAsync(Routing.MarkerHub.Client.ReceiveSpikeBroadcast, new SpikeBroadcast
        {
            Id = Context.ConnectionId,
            Latitude = spike.Latitude,
            Longitude = spike.Longitude,
            UpdatedOn = spike.UpdatedOn,
        });
    }

    //[HubMethodName(Routing.MarkerHub.Server.RemoveMarker)]
    //public async Task RemoveMarker(string key) {
    //    Markers.TryRemove(key, out _);
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, key);
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, key);
    //}

    [HubMethodName(Routing.MarkerHub.Server.BroadcastInfoMarker)]
    public async Task BroadcastInfoMarker(InfoMarker infoMarker)
    {
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

    //[HubMethodName(Routing.MarkerHub.Server.UpdateConnectionId)]
    //public async Task UpdateConnectionId(string oldConnectionId, Team team, string labelColor) 
    //{
    //    if(Markers.TryRemove(oldConnectionId, out var oldMarker)) {
    //        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, oldConnectionId);
    //        await BroadcastMarker(oldMarker with {
    //            Id = Context.ConnectionId,
    //            Connected = true,
    //            Team = team,
    //            Color = labelColor
    //        });
    //    }
    //    if(InfoMarkers.TryRemove(oldConnectionId, out var oldInfoMarker)) {
    //        await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, oldConnectionId);
    //        await BroadcastInfoMarker(oldInfoMarker with {
    //            Id = Context.ConnectionId,
    //            Team = team,
    //            LabelColor = labelColor
    //        });
    //    }
    //}
}
