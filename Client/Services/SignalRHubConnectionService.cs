using Constants = BluForTracker.Shared.Constants;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;

namespace BluForTracker.Client.Shared.Services;

public class SignalRHubConnectionService : IAsyncDisposable
{
    private HubConnection _hubConnection { get; }
    private IHubConnectionBuilder HubConnectionBuilder { get; }
    public SignalRHubConnectionService(Uri endpoint) {
        var endpointBuilder = new UriBuilder(endpoint);
        endpointBuilder.Path += Constants.MarkerHubPath;
        endpointBuilder.Query = $"{Constants.Secret.Key}={Constants.Secret.Value}";
        HubConnectionBuilder = new HubConnectionBuilder()
            .WithUrl(endpointBuilder.Uri, options => {
                options.Transports = HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect();
        _hubConnection = HubConnectionBuilder.Build();
    }

    public async Task<HubConnection> GetHubConnection()
    {
        if(_hubConnection?.State == HubConnectionState.Disconnected) await _hubConnection.StartAsync();
        return _hubConnection ?? throw new NullReferenceException("HubConnection was null when it should not be");
    }

    public string Status() => _hubConnection.State.ToString();
    public async ValueTask DisposeAsync()
    {
        if(_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
