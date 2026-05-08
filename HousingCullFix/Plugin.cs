using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using HousingCullFix.Fixes;
using HousingCullFix.Utils;

namespace HousingCullFix;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    private const string CommandName = "/housingcullfix";
    
    public Configuration Configuration { get; init; }

    public HouseFunctions HouseFunctions { get; set; }
    
    public readonly List<IFix> Fixes;
    public int FixIndex { get; set; } = -1;

    public readonly WindowSystem WindowSystem = new("HousingCullFix");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        HouseFunctions = new HouseFunctions();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Manage Housing Cull Fix settings"
        });
        
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        Fixes = [
            new FakeOutside(),
            new HookAndChange()
        ];
        
        SetFix(Configuration.SelectedFix);
        Scene.SetCastShadows(Configuration.EnableCastShadows);
    }
    
    private void OnCommand(string command, string arguments) => ConfigWindow.Toggle();
    private void ToggleConfigUi() => ConfigWindow.Toggle();

    public void SetFix(string assemblyName)
    {
        foreach (var fix in Fixes) 
            if (fix.Enabled)
            {
                fix.Disable();
                Log.Verbose($"Disabled {fix.Name}");
            }
        
        for (var i = 0; i < Fixes.Count; i++)
        {
            var item = Fixes[i];
            if (item.GetType().Name == assemblyName)
            {
                item.Enable();
                Log.Verbose($"Enabled {item.Name}");
                
                FixIndex = i;
                WarnOfFix();
                return;
            }
        }
        
        FixIndex = -1; // fix was not found
        Log.Verbose("No fix selected.");
    }

    public static unsafe void WarnOfFix()
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        Toast.Warning("Some objects may have been reverted to their default state. Please re-enter the area to fix.");
    }
    
    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);
        
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();
        
        ConfigWindow.Dispose();

        foreach (var fix in Fixes)
        {
            fix.Dispose();
            Log.Verbose($"Disposed {fix.Name}");
        }
        
        Scene.SetCastShadows(true);
        
        Log.Verbose("Plugin disposed.");
    }
}
