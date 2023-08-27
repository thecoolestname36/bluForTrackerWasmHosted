using BluForTracker.Shared;
using Microsoft.AspNetCore.SignalR;

namespace BluForTracker.Server;

public class LocationHub : Hub
{
    public async Task BroadcastLocation(Marker marker)
    {
        await Clients.All.SendAsync("NotifyLocation", Context.ConnectionId, marker);
    }
}
