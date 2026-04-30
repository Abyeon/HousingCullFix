using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace HousingCullFix;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private CullHook CullHook { get; init; } = new();
    
    public void Dispose()
    {
        CullHook.Dispose();
        Log.Verbose("Plugin disposed.");
    }
}
