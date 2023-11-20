using BluForTracker.Client.Shared.Services;
using BluForTracker.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel.DataAnnotations;

namespace BluForTracker.Client.Shared.Components;

public partial class TeamsView
{
    [Inject] public required AppStateService AppStateService { get; set; }
    [Inject] public required SignalRHubConnectionService HubConnectionService { get; set; }
    private List<TeamRoster> _teamRosters { get; set; } = new();

    private TeamFormModel _formData = new();
    class TeamFormModel
    {
        public const int TeamNameMaxChars = 28;
        [StringLength(TeamNameMaxChars, ErrorMessage = "Team name must be 28 characters or less.")]
        public string TeamName { get; set; } = "";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if(firstRender)
        {
            // This will render when the tab is selected
            var hubConnection = await HubConnectionService.GetHubConnection();
            if(hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("GetTeamRosters");
            }
        }
    }

    public async Task OnTeamRosterChanged() {
        var hubConnection = await HubConnectionService.GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("GetTeamRosters");
        }
    }

    public void OnReceiveTeamRosters(List<TeamRoster> teamRosters) {
        _teamRosters.Clear();
        _teamRosters.AddRange(teamRosters);
        StateHasChanged();
    }

    public void OnJoinedTeam(Guid teamId) => AppStateService.GetUser().TeamId = teamId;

    async Task OnValidSubmit()
    {
        var teamName = _formData.TeamName;
        _formData.TeamName = "";
        var hubConnection = await HubConnectionService.GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("CreateAndJoinTeam", teamName);
        }
    }

    async Task JoinTeamClicked(Guid teamId)
    {
        var hubConnection = await HubConnectionService.GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("JoinTeam", teamId);
        }
    }

    async Task RemoveUser(string? connectionId)
    {
        if(string.IsNullOrEmpty(connectionId)) return;
        var hubConnection = await HubConnectionService.GetHubConnection();
        if(hubConnection.State == HubConnectionState.Connected)
        {
            await hubConnection.SendAsync("RemoveUser", connectionId);
        }
    }
}
