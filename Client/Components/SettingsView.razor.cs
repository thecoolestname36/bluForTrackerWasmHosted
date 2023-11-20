using System.ComponentModel.DataAnnotations;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using BluForTracker.Client.Shared.Services;

namespace BluForTracker.Client.Shared.Components;

public partial class SettingsView : IDisposable
{
    [Inject] public required SignalRHubConnectionService HubConnectionService { get; set; }
    [Inject] public required IGeolocationService GeolocationService { get; set; }
    [Inject] public required AppStateService AppState { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Parameter] public EventCallback OnValidSubmitCallback { get; set; } = EventCallback.Empty;
    ElementReference _geolocationStatusRef;
    ElementReference _hubConnectionStatusRef;
    ElementReference _hubConnectionIdRef;
    private IJSObjectReference? _collocatedJs;
    private MarkerFormModel _formData = new();

    class MarkerFormModel
    {
        public const int UsernameMaxChars = 28;

        [StringLength(UsernameMaxChars, ErrorMessage = "Username must be 28 characters or less.")]
        public string Username { get; set; } = "";

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid Format")]
        public string Color { get; set; } = "#000000";
    }

    protected override async void OnParametersSet()
    {
        base.OnParametersSet();
        _formData.Username = AppState.GetUser().Username;
        _formData.Color = AppState.GetUser().Color;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if(firstRender)
        {
            _collocatedJs = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BluForTracker.Client.Shared/Components/SettingsView.razor.js");
            StatusLoop();
        }
    }

    PeriodicTimer? _statusTimer;
    async void StatusLoop()
    {
        _statusTimer ??= new(TimeSpan.FromSeconds(1));
        while(await _statusTimer.WaitForNextTickAsync())
        {
            if(_collocatedJs != null)
            {
                await _collocatedJs.InvokeVoidAsync("setElementValue", _geolocationStatusRef, (await GeolocationService.Status()) ?? "Unknown");
                await _collocatedJs.InvokeVoidAsync("setElementValue", _hubConnectionStatusRef, (await HubConnectionService.GetHubConnection()).State.ToString());
                await _collocatedJs.InvokeVoidAsync("setElementValue", _hubConnectionIdRef, AppState.GetUser()?.ConnectionId ?? "Unknown");
            }
        }
    }

    private async Task OnValidSubmit()
    {
        var user = AppState.GetUser();
        user.Username = _formData.Username;
        user.Color = _formData.Color;
        var hubConnection = await HubConnectionService.GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("UpdateUser", AppState.GetUser());
        }
        await OnValidSubmitCallback.InvokeAsync();
    }

    public void Dispose()
    {
        _statusTimer?.Dispose();
    }
}
