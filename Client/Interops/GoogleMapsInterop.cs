using System.Runtime.Versioning;
using Microsoft.JSInterop;

namespace BluForTracker.Client.Interops;

[SupportedOSPlatform("browser")]
public class GoogleMapsInterop(IJSRuntime jsRuntime)
{
    public async Task<IJSObjectReference> CreateObject()
    {
        return await jsRuntime.InvokeAsync<IJSObjectReference>("globalThis.bluForTracker.googleMaps.CreateMapModule");
    }
}
