using BluForTracker.Client.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BluForTracker.Client.Shared;

public static class AppBuilderExtensions
{
    public static void AddSignalRHubConnection(this IServiceCollection source, Uri endpoint) => source.AddScoped(provider => new SignalRHubConnectionService(endpoint));
}
