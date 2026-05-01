using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace HousingCullFix;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;

    private const string CommandName = "/housingcullfix";
    
    public Configuration Configuration { get; init; }

    private CullHook CullHook { get; init; }

    public readonly WindowSystem WindowSystem = new("HousingCullFix");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        CullHook = new CullHook();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Manage Housing Cull Fix settings"
        });
        
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }
    
    private void OnCommand(string command, string arguments) => ConfigWindow.Toggle();
    private void ToggleConfigUi() => ConfigWindow.Toggle();

    public static unsafe void SetShadowLights(bool enabled)
    {
        var config = GraphicsConfig.Instance();
        if (config == null) throw new NullReferenceException("GraphicsConfig.Instance() returned null");
        
        var man = HousingManager.Instance();
        if (man == null) return;

        if (!enabled && man->IsInside())
        {
            config->ShadowLightValidType = 0;
        }
        else
        {
            if (GameConfig.System.TryGetUInt("ShadowLightValidType", out var maxShadows))
            {
                config->ShadowLightValidType = maxShadows switch
                {
                    0 => 8,
                    1 => 14,
                    2 => 20,
                    _ => throw new ArgumentOutOfRangeException($"ShadowLightValidType returned an unexpected value {maxShadows}")
                };
            }
            else
            {
                Log.Error("Could not find ShadowLightValidType.");
            }
        }
    }
    
    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);
        
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();
        
        ConfigWindow.Dispose();
        
        SetShadowLights(true);
        
        CullHook.Dispose();
        Log.Verbose("Plugin disposed.");
    }
}
