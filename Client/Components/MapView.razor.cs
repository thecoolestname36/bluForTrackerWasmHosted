using BluForTracker.Client.Shared.Services;
using BluForTracker.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BluForTracker.Client.Shared.Components;

public partial class MapView : IAsyncDisposable
{
    [Inject] public required NavigationManager NavManager { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] public required IAppLocalStorageService LocalStorage { get; set; }
    [Inject] public required IGeolocationService GeolocationService { get; set; }
    [Inject] public required SignalRHubConnectionService SignalRHubConnectionService { get; set; }
    [Inject] public required AppStateService AppState { get; set; }

    IJSObjectReference? _mapModuleJs = default!;

    public async Task UserUpdated()
    {
        var hubConnection = await GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("UpdateUser", AppState.GetUser());
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await GetHubConnection();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if(firstRender)
        {
            var collocatedJs = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BluForTracker.Client.Shared/Components/MapView.razor.js");
            _mapModuleJs = await collocatedJs.InvokeAsync<IJSObjectReference>("CreateMapModule");
            await MapLoadLoop();
        }
    }

    PeriodicTimer? _mapLoadTimer;
    async Task MapLoadLoop()
    {
        _mapLoadTimer ??= new(TimeSpan.FromMilliseconds(250));
        while(await _mapLoadTimer.WaitForNextTickAsync())
        {
            if(await _mapModuleJs.InvokeAsync<bool>("isGoogleMapsApiLoading")) continue;

            await _mapModuleJs.InvokeVoidAsync("setupMapModule", DotNetObjectReference.Create(this));
            _mapLoadTimer.Dispose();
            _mapLoadTimer = null;
            return;
        }
    }

    [JSInvokable]
    public async Task MapModuleSetupComplete()
    {
        var hubConnection = await GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("JoinTeam", AppState.GetUser().TeamId);
            MyLocationLoop();
        }
    }

    MapMarker? _lastSentLocation;
    PeriodicTimer? _myLocationTimer;
    async void MyLocationLoop()
    {
        _myLocationTimer ??= new(TimeSpan.FromMilliseconds(1000));
        var sending = false;
        while(await _myLocationTimer.WaitForNextTickAsync())
        {
            if(sending) continue;
            var location = await GeolocationService.GetCurrentLocation();
            if(location == null || _lastSentLocation != null && _lastSentLocation.Timestamp == location.Timestamp) continue;

            var hubConnection = await GetHubConnection();
            if(hubConnection.State == HubConnectionState.Connected)
            {
                if(_lastSentLocation == null)
                {
                    await SetCenter(location);
                }
                _lastSentLocation = (_lastSentLocation ?? new MapMarker()) with
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Timestamp = location.Timestamp
                };
                sending = true;
                await hubConnection.SendAsync("BroadcastMapMarker", _lastSentLocation);
                sending = false;
            }
        }
    }

    public async Task<HubConnection> GetHubConnection()
    {
        if(SignalRHubConnectionService.HubConnection == null)
        {
            var hubConnection = await SignalRHubConnectionService.Init();
            hubConnection.On<string>("ReceiveConnectionId", async (connectionId) => 
            {
                AppState.GetUser().ConnectionId = connectionId;
                // Maybe do some stuff here like check storage for an existing connection ID and tell the server to update it

            });
            hubConnection.On<List<User>>("ReceiveTeam", async (users) => 
            {
                if(_mapModuleJs == null) return;
                await _mapModuleJs.InvokeVoidAsync("receiveTeam", users.Select(user => user.ToGoogleMapMarker()).Where(user => user != null));
            });
            hubConnection.On<User>("ReceiveTeamMember", async (user) => 
            {
                var mapMarker = user.ToGoogleMapMarker();
                if(_mapModuleJs == null || mapMarker == null) return;
                await _mapModuleJs.InvokeVoidAsync("receiveTeamMember", mapMarker);
            });
            hubConnection.On<string, MapMarker>("ReceiveMapMarker", async (connectionId, mapMarker) =>
            {
                if(_mapModuleJs == null) return;
                await _mapModuleJs.InvokeVoidAsync("receiveMapMarker", connectionId, mapMarker);
            });
            hubConnection.On<string>("RemoveTeamMember", async (connectionId) => 
            {
                if(_mapModuleJs == null) return;
                await _mapModuleJs.InvokeVoidAsync("removeTeamMember", connectionId);
            });
        }
        if(SignalRHubConnectionService.HubConnection?.State == HubConnectionState.Disconnected) await SignalRHubConnectionService.HubConnection.StartAsync();
        return SignalRHubConnectionService.HubConnection ?? throw new NullReferenceException("HubConnection was null when it should not be");
    }

    async Task SetCenter(MapMarker marker) {
        if(_mapModuleJs != null) {
            await _mapModuleJs.InvokeVoidAsync("setCenter", marker.Latitude, marker.Longitude);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if(SignalRHubConnectionService.HubConnection != null) await SignalRHubConnectionService.HubConnection.DisposeAsync();
        if(_mapModuleJs != null)
        {
            await _mapModuleJs.DisposeAsync();
            _mapModuleJs = null;
        }
        _mapLoadTimer?.Dispose();
        _mapLoadTimer = null;
        _myLocationTimer?.Dispose();
        _myLocationTimer = null;
    }



    //onReceiveConnectionId: async (connectionId) =>
    //{
    //    AppState.GetUser().Id = connectionId;
    //    // Maybe do some stuff here like check storage for an existing connection ID and tell the server to update it
    //    await Task.CompletedTask;
    //},
    //onReceiveSpikeBroadcast: async (SpikeBroadcast spikeBroadcast) =>
    //{
    //    _markers.AddOrUpdate(spikeBroadcast.Id, new Marker
    //    {
    //        Id = spikeBroadcast.Id,
    //        Spike = new Spike
    //        {
    //            Latitude = spikeBroadcast.Latitude,
    //            Longitude = spikeBroadcast.Longitude,
    //            UpdatedOn = spikeBroadcast.UpdatedOn
    //        }
    //    }, (key, oldValue) =>
    //    {
    //        oldValue.Spike.Latitude = spikeBroadcast.Latitude;
    //        oldValue.Spike.Longitude = spikeBroadcast.Longitude;
    //        oldValue.Spike.UpdatedOn = spikeBroadcast.UpdatedOn;
    //        return oldValue;
    //    });
    //    if(_markers.TryGetValue(spikeBroadcast.Id, out var marker) && _mapModuleJs != null) await _mapModuleJs.InvokeVoidAsync("receiveMarker", marker);
    //},
    //onReceiveMarker: async (marker) =>
    //{
    //    _markers[marker.Id] = marker;
    //    //StateHasChanged();
    //    await _mapModuleJs.InvokeVoidAsync("receiveMarker", marker);
    //},
    //onReceiveMarkers: async (markers) =>
    //{
    //    _markers = new ConcurrentDictionary<string, Marker>(markers.Select(marker => new KeyValuePair<string, Marker>(marker.Id, marker)));
    //    //StateHasChanged();
    //    await _mapModuleJs.InvokeVoidAsync("syncMarkers", _markers.Values.ToList());
    //},
    //onRemoveMarker: async (key) =>
    //{
    //    _markers.TryRemove(key, out _);
    //    //StateHasChanged();
    //    await _mapModuleJs.InvokeVoidAsync("removeMarker", key);
    //},
    //onReceiveInfoMarker: async (infoMarker) =>
    //{
    //    await _mapModuleJs.InvokeVoidAsync("receiveInfoMarker", infoMarker);
    //},
    //onReceiveInfoMarkers: async (infoMarkers) =>
    //{
    //    await _mapModuleJs.InvokeVoidAsync("syncInfoMarkers", infoMarkers.ToList());
    //},
    //onRemoveInfoMarker: async (key) =>
    //{
    //    await _mapModuleJs.InvokeVoidAsync("removeInfoMarker", key);
    //}
    //);

    //[JSInvokable]
    //public async Task BroadcastInfoMarker(string message, double lat, double lng) {
    //    if(AppState.GetUser().Id == null) return;
    //    var hubConnection = await GetHubConnection();
    //    if(hubConnection.State == HubConnectionState.Connected) await hubConnection.SendAsync(Routing.MarkerHub.Server.BroadcastInfoMarker, new MessageSpike() {
    //        Label = AppState.GetUser().Username ?? "unk",
    //        LabelColor = AppState.GetUser().Color ?? "#000000",
    //        Message = string.IsNullOrEmpty(message) ? "" : message + "<br/>",
    //        Spike = new Spike
    //        {
    //            Latitude = lat,
    //            Longitude = lng,
    //            UpdatedOn = DateTimeOffset.UtcNow
    //        }
    //    });
    //}

    //[JSInvokable]
    //public async Task RemoveInfoMarker(string key)
    //{
    //    var hubConnection = await GetHubConnection();
    //    if(hubConnection.State == HubConnectionState.Connected)
    //    {
    //        await hubConnection.SendAsync(Routing.MarkerHub.Server.RemoveInfoMarker, key);
    //    }
    //}

    //async Task RemoveMarker(string key)
    //{
    //    var hubConnection = await GetHubConnection();
    //    if(hubConnection.State == HubConnectionState.Connected)
    //    {
    //        await hubConnection.SendAsync(Routing.MarkerHub.Server.RemoveMarker, key);
    //    }
    //}
}
