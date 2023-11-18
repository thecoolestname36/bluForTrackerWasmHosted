using BluForTracker.Shared;
using Constants = BluForTracker.Shared.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BluForTracker.Server;

public class MarkerHub : Hub
{
    public static readonly ConcurrentDictionary<string, User> Users = new();
    private static readonly Team _defaultTeam = new()
    {
        Id = Guid.Empty,
        Name = "Default"
    };
    public static readonly ConcurrentDictionary<Guid, Team> Teams = new();

    public MarkerHub() => GCLoop();

    PeriodicTimer? _gcTimer;
    async Task GCLoop()
    {
        bool working = false;
        _gcTimer ??= new(TimeSpan.FromSeconds(10));
        while(await _gcTimer.WaitForNextTickAsync())
        {
            if(working == true) continue;
            working = true;
            try
            {
                var removeTeams = new List<Guid>(Teams.Keys.ToList());
                // If we havent retrieved the user in (diff) time, we can assume they are timed out and we need to remove them from the list of Users.
                foreach(var user in Users)
                {
                    if(DateTimeOffset.UtcNow - user.Value.Timestamp > TimeSpan.FromMinutes(60))
                    {
                        if(Users.TryRemove(user)) {
                            await Clients.Group(user.Value.TeamId.ToString()).SendAsync("RemoveTeamMember", user.Value.ConnectionId);
                        }
                    } else
                    {
                        // If the user is active, their team is active
                        removeTeams.Remove(user.Value.TeamId);
                    }
                }
                // TODO
                //foreach(var removeTeam in removeTeams)
                //{
                //    if(removeTeam == Guid.Empty) continue;
                //    Teams.Remove(removeTeam, out _);
                //}
            } catch(Exception e)
            {
                _ = e;
            } finally {
                working = false;
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            await base.OnConnectedAsync();
            var httpCtx = Context.GetHttpContext();
            var secret = httpCtx?.Request.Query[Constants.Secret.Key];
            if(!secret.HasValue || secret.Value != Constants.Secret.Value) {
                Context.Abort();
                return;
            }
            Users[Context.ConnectionId] = new User()
            {
                ConnectionId = Context.ConnectionId,
                Color = "#000000",
            };
            await Clients.Caller.SendAsync("ReceiveConnectionId", Context.ConnectionId);
        } catch(Exception e)
        {
            _ = e;
        }
    }

    public async Task JoinTeam(Guid teamId) => await SafeClientUserHandler(async (user) =>
    {
        if(!Teams.ContainsKey(Guid.Empty))
        {
            Teams[_defaultTeam.Id] = _defaultTeam;
        }
        if(Teams.ContainsKey(teamId))
        {
            user.TeamId = teamId;
            await Groups.AddToGroupAsync(Context.ConnectionId, teamId.ToString());
            await Clients.Group(teamId.ToString()).SendAsync("ReceiveTeamMember", user);
            await Clients.Caller.SendAsync("ReceiveTeam", Users
                .Where(u => u.Value.TeamId == user.TeamId)
                .Select(u => u.Value)
                .ToList());
        }
    });

    public async Task BroadcastMapMarker(MapMarker marker) => await SafeClientUserHandler(async (user) => 
    {
        marker.Timestamp = DateTime.UtcNow;
        user.MapMarker = marker;
        await Clients.Group(user.TeamId.ToString()).SendAsync("ReceiveMapMarker", Context.ConnectionId, user.MapMarker);
    });

    public async Task BroadcastInfoMarker(InfoMarker messageMarker) => await SafeClientUserHandler(async (user) => 
    {
        messageMarker.Timestamp = DateTime.UtcNow;
        user.InfoMarker = messageMarker;
        await Clients.Group(user.TeamId.ToString()).SendAsync("ReceiveInfoMarker", Context.ConnectionId, user.InfoMarker);
    });

    //public async Task RequestTeam() => await SafeClientUserHandler(async (user) => await Clients.Caller.SendAsync("ReceiveTeam",
    //    //arg1: Teams
    //    //    .Where(t => t.Value.Id == user.TeamId)
    //    //    .Select(t => t.Value)
    //    //    .FirstOrDefault(),
    //    arg1: Users
    //        .Where(u => u.Value.TeamId == user.TeamId)
    //        .Select(u => u.Value)
    //        .ToList()));

    public async Task UpdateUser(User userUpdates) => await SafeClientUserHandler(async (user) => 
    {
        user.Username = userUpdates.Username;
        user.Color = userUpdates.Color;
        await Clients.Group(user.TeamId.ToString()).SendAsync("ReceiveTeamMember", user);
    });


    async Task SafeClientUserHandler(Func<User, Task> fn) 
    {
        try
        {
            if(Users.TryGetValue(Context.ConnectionId, out var user) && user != null)
            {
                user.Timestamp = DateTimeOffset.UtcNow;
                await fn(user);
            }
        } catch(Exception e)
        {
            _ = e;
        }
    }

    //[HubMethodName(Routing.MarkerHub.Server.AddToGroup)]
    //public async Task AddToGroup(Guid groupId)
    //{
    //    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
    //    //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    //}

    //[HubMethodName(Routing.MarkerHub.Server.RemoveFromGroup)]
    //public async Task RemoveFromGroup(Guid groupId)
    //{
    //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
    //    //await Clients.Group(groupId).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    //}

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



    //[HubMethodName(Routing.MarkerHub.Server.RemoveMarker)]
    //public async Task RemoveMarker(string key) {
    //    Markers.TryRemove(key, out _);
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveMarker, key);
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, key);
    //}

    //public async Task RemoveInfoMarker(string key)
    //{
    //    InfoMarkers.TryRemove(key, out _);
    //    await Clients.All.SendAsync(Routing.MarkerHub.Client.RemoveInfoMarker, key);
    //}

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
