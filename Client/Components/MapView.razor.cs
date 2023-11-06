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
    ConcurrentDictionary<string, Marker> _markers = new();

    public async Task<HubConnection> GetHubConnection()
    {
        if(SignalRHubConnectionService.HubConnection == null)
        {
            await SignalRHubConnectionService.Init(
                onReceiveSync: async (model) =>
                {
                    AppState.User.Id = model.ConnectionId;

                    
                    await _mapModuleJs.InvokeVoidAsync("syncMarkers", model.Spikes);
                    await _mapModuleJs.InvokeVoidAsync("syncInfoMarkers", model.InfoMarkers);
                    
                    await Task.CompletedTask;
                },
                onReceiveSpikeBroadcast: async (SpikeBroadcast spikeBroadcast) =>
                {
                    _markers.AddOrUpdate(spikeBroadcast.Id, new Marker
                    {
                        Id = spikeBroadcast.Id,
                        Spike = new Spike
                        {
                            Latitude = spikeBroadcast.Latitude,
                            Longitude = spikeBroadcast.Longitude,
                            UpdatedOn = spikeBroadcast.UpdatedOn
                        }
                    }, (key, oldValue) =>
                    {
                        oldValue.Spike.Latitude = spikeBroadcast.Latitude;
                        oldValue.Spike.Longitude = spikeBroadcast.Longitude;
                        oldValue.Spike.UpdatedOn = spikeBroadcast.UpdatedOn;
                        return oldValue;
                    });
                    if(_markers.TryGetValue(spikeBroadcast.Id, out var marker) && _mapModuleJs != null) await _mapModuleJs.InvokeVoidAsync("receiveMarker", marker);
                },
                onReceiveMarker: async (marker) =>
                {
                    _markers[marker.Id] = marker;
                    //StateHasChanged();
                    await _mapModuleJs.InvokeVoidAsync("receiveMarker", marker);
                },
                onReceiveMarkers: async (markers) =>
                {
                    _markers = new ConcurrentDictionary<string, Marker>(markers.Select(marker => new KeyValuePair<string, Marker>(marker.Id, marker)));
                    //StateHasChanged();
                    await _mapModuleJs.InvokeVoidAsync("syncMarkers", _markers.Values.ToList());
                },
                onRemoveMarker: async (key) =>
                {
                    _markers.TryRemove(key, out _);
                    //StateHasChanged();
                    await _mapModuleJs.InvokeVoidAsync("removeMarker", key);
                },
                onReceiveInfoMarker: async (infoMarker) =>
                {
                    await _mapModuleJs.InvokeVoidAsync("receiveInfoMarker", infoMarker);
                },
                onReceiveInfoMarkers: async (infoMarkers) =>
                {
                    await _mapModuleJs.InvokeVoidAsync("syncInfoMarkers", infoMarkers.ToList());
                },
                onRemoveInfoMarker: async (key) =>
                {
                    await _mapModuleJs.InvokeVoidAsync("removeInfoMarker", key);
                }
            );
        }
        if(SignalRHubConnectionService.HubConnection?.State == HubConnectionState.Disconnected) await SignalRHubConnectionService.HubConnection.StartAsync();
        return SignalRHubConnectionService.HubConnection ?? throw new NullReferenceException("HubConnection was null when it should not be");
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
            MyLocationLoop();
        }
    }

    PeriodicTimer? _mapLoadTimer;
    async Task MapLoadLoop()
    {
        _mapLoadTimer ??= new(TimeSpan.FromMilliseconds(250));
        while(await _mapLoadTimer.WaitForNextTickAsync())
        {
            if(await _mapModuleJs.InvokeAsync<bool>("isGoogleMapsApiLoading")) continue;

            await _mapModuleJs.InvokeVoidAsync("addListenerOnceMapIdle", DotNetObjectReference.Create(this));
            _mapLoadTimer.Dispose();
            _mapLoadTimer = null;
            return;
        }
    }

    Spike? _lastSentLocation;
    PeriodicTimer? _myLocationTimer;
    async void MyLocationLoop()
    {
        _myLocationTimer ??= new(TimeSpan.FromMilliseconds(1000));
        var sending = false;
        while(await _myLocationTimer.WaitForNextTickAsync())
        {
            if(sending) continue;
            var location = await GeolocationService.GetCurrentLocation();
            if(location == null || _lastSentLocation != null && _lastSentLocation.UpdatedOn == location.UpdatedOn) continue;

            var hubConnection = await GetHubConnection();
            if(hubConnection.State == HubConnectionState.Connected)
            {
                if(_lastSentLocation == null)
                {
                    await SetCenter(location);
                }
                _lastSentLocation = (_lastSentLocation ?? new Spike()) with
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    UpdatedOn = location.UpdatedOn
                };
                sending = true;
                await hubConnection.SendAsync(Routing.MarkerHub.Server.BroadcastSpike, _lastSentLocation);
                sending = false;
            }
        }
    }

    [JSInvokable]
    public async Task BroadcastInfoMarker(string message, double lat, double lng) {
        if(AppState.User.Id == null) return;
        var hubConnection = await GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected) await hubConnection.SendAsync(Routing.MarkerHub.Server.BroadcastInfoMarker, new InfoMarker() {
            Label = AppState.User?.Label ?? "unk",
            LabelColor = AppState.User?.Color ?? "#000000",
            Message = string.IsNullOrEmpty(message) ? "" : message + "<br/>",
            Spike = new Spike
            {
                Latitude = lat,
                Longitude = lng,
                UpdatedOn = DateTimeOffset.UtcNow
            }
        });
    }

    [JSInvokable]
    public async Task RemoveInfoMarker(string key)
    {
        var hubConnection = await GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync(Routing.MarkerHub.Server.RemoveInfoMarker, key);
        }
    }

    async Task RemoveMarker(string key)
    {
        var hubConnection = await GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync(Routing.MarkerHub.Server.RemoveMarker, key);
        }
    }

    async Task SetCenter(Spike spike) {
        if(_mapModuleJs != null) {
            await _mapModuleJs.InvokeVoidAsync("setCenter", spike.Latitude, spike.Longitude);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if(SignalRHubConnectionService.HubConnection != null) await SignalRHubConnectionService.HubConnection.DisposeAsync();
        _markers.Clear();
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
}
