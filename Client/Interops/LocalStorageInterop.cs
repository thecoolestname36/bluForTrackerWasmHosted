using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
namespace BluForTracker.Client.Interops;

[SupportedOSPlatform("browser")]
public static partial class LocalStorageInterop
{
    [JSImport("globalThis.localStorage.setItem")]
    internal static partial void SetItem(string key, string value);
    
    [JSImport("globalThis.localStorage.getItem")]
    internal static partial string? GetItem(string key);
    
    [JSImport("globalThis.localStorage.removeItem")]
    internal static partial void RemoveItem(string key);
    
    [JSImport("globalThis.localStorage.clear")]
    internal static partial void Clear();
}