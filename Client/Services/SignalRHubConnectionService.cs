using BluForTracker.Shared;
using Constants = BluForTracker.Shared.Constants;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using static BluForTracker.Client.Shared.Services.SignalRHubConnectionService;

namespace BluForTracker.Client.Shared.Services;

public class SignalRHubConnectionService : IAsyncDisposable
{
    public HubConnection? HubConnection { get; private set; }
    private IHubConnectionBuilder HubConnectionBuilder { get; }
    public SignalRHubConnectionService(Uri endpoint) {
        var endpointBuilder = new UriBuilder(endpoint);
        endpointBuilder.Path += Routing.MarkerHub.Path;
        endpointBuilder.Query = $"{Constants.Secret.Key}={Constants.Secret.Value}";
        HubConnectionBuilder = new HubConnectionBuilder()
            .WithUrl(endpointBuilder.Uri, options => {
                options.Transports = HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect();
    }

    public delegate Task OnReceiveSync(SyncModel model);
    public delegate Task OnReceiveMarker(Marker marker);
    public delegate Task OnReceiveMarkers(List<Marker> marker);
    public delegate Task OnReceiveSpikeBroadcast(SpikeBroadcast spikeBroadcast);
    public delegate Task OnRemoveMarker(string key);
    public delegate Task OnReceiveInfoMarker(InfoMarker infoMarker);
    public delegate Task OnReceiveInfoMarkers(List<InfoMarker> infoMarkers);
    public delegate Task OnRemoveInfoMarker(string key);

    public async Task<SignalRHubConnectionService> Init(
            OnReceiveSync? onReceiveSync = null,
            OnReceiveMarker? onReceiveMarker = null,
            OnReceiveMarkers? onReceiveMarkers = null,
            OnReceiveSpikeBroadcast? onReceiveSpikeBroadcast = null,
            OnRemoveMarker? onRemoveMarker = null,
            OnReceiveInfoMarker? onReceiveInfoMarker = null,
            OnReceiveInfoMarkers? onReceiveInfoMarkers = null,
            OnRemoveInfoMarker? onRemoveInfoMarker = null) {
        if(HubConnection != null) await HubConnection.DisposeAsync().AsTask();
        HubConnection = HubConnectionBuilder.Build();
        if(onReceiveSync != null) HubConnection.On<SyncModel>(Routing.MarkerHub.Client.ReceiveSync, async (model) => await onReceiveSync(model));
        if(onReceiveMarker != null) HubConnection.On<Marker>(Routing.MarkerHub.Client.ReceiveMarker, async (marker) => await onReceiveMarker(marker));
        if(onReceiveMarkers != null) HubConnection.On<List<Marker>>(Routing.MarkerHub.Client.ReceiveMarkers, async (markers) => await onReceiveMarkers(markers));
        if(onReceiveSpikeBroadcast != null) HubConnection.On<SpikeBroadcast>(Routing.MarkerHub.Client.ReceiveSpikeBroadcast, async (spikeBroadcast) => await onReceiveSpikeBroadcast(spikeBroadcast));
        if(onRemoveMarker != null) HubConnection.On<string>(Routing.MarkerHub.Client.RemoveMarker, async (key) => await onRemoveMarker(key));
        if(onReceiveInfoMarker != null) HubConnection.On<InfoMarker>(Routing.MarkerHub.Client.ReceiveInfoMarker, async (infoMarker) => await onReceiveInfoMarker(infoMarker));
        if(onReceiveInfoMarkers != null) HubConnection.On<List<InfoMarker>>(Routing.MarkerHub.Client.ReceiveInfoMarkers, async (infoMarkers) => await onReceiveInfoMarkers(infoMarkers));
        if(onRemoveInfoMarker != null) HubConnection.On<string>(Routing.MarkerHub.Client.RemoveInfoMarker, async (key) => await onRemoveInfoMarker(key));
        return this;
    }

    public string Status() => HubConnection == null ? "Init required" : HubConnection.State.ToString();
    public async ValueTask DisposeAsync()
    {
        if(HubConnection != null) {
            await HubConnection.StopAsync();
            await HubConnection.DisposeAsync();
        }
    }
}