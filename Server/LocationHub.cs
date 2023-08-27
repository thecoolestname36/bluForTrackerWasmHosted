using BluForTracker.Shared;
using Microsoft.AspNetCore.SignalR;

namespace BluForTracker.Server;

public class LocationHub : Hub
{
    public async Task UserConnected() {
        await Clients.Caller.SendAsync("UserConnectedCallback", Context.ConnectionId);
    }

    public async Task BroadcastLocation(Marker marker)
    {
        var now = DateTimeOffset.UtcNow;
        marker.UpdatedOn = now;
        await Clients.All.SendAsync("NotifyLocation", now, marker);
    }
}
