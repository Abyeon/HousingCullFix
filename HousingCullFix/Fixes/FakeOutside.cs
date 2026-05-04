using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.String;
using HousingCullFix.Structs;
using HousingCullFix.Utils;
using InteropGenerator.Runtime;

namespace HousingCullFix.Fixes;

public unsafe class FakeOutside : IFix
{
    public string Name { get; init; } = "Fake Outside (Recommended)";

    public string Description { get; init; } = "Flips a \"IsInside\" boolean the game checks before running their new culling algorithm.\n" +
                                               "This has the side benefit of re-enabling void lighting.";
    
    public bool Enabled { get; set; }
    
    private delegate void LoadZoneDelegate(LayoutManager* layoutManager, int id, CStringPointer bg, CStringPointer bgNoExtension, int territoryType, int layerFilterKey, int type, GameMain.Festival[] festivals, int cfcId);

    [Signature("40 53 48 83 EC ?? 8B 44 24 ?? 48 8B D9 89 41", DetourName = nameof(LoadZoneDetour))]
    private readonly Hook<LoadZoneDelegate>? loadZoneHook = null!;

    public FakeOutside()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }
    
    public void Enable()
    {
        loadZoneHook?.Enable();
        Plugin.Framework.Run(() => ToggleCulling(false));
        Enabled = true;

        Plugin.Log.Debug("Enabled Fake Outside fix.");
    }

    public void Disable()
    {
        loadZoneHook?.Disable();
        Plugin.Framework.Run(() => ToggleCulling(true));
        Enabled = false;
        
        Plugin.Log.Debug("Disabled Fake Outside fix.");
    }
    
    public void LoadZoneDetour(LayoutManager* layoutManager, int id, CStringPointer bg, CStringPointer bgNoExtension, int territoryType, int layerFilterKey, int type, GameMain.Festival[] festivals, int cfcId)
    {
        Plugin.Log.Verbose($"Loading: {bg}");
        
        if (bgNoExtension.ToString().Contains("/ind/"))
        {
            var newString = bgNoExtension.ToString().Replace("/ind/", "/hehe/"); // Just swapping this is okay :)
            fixed (char* charPtr = newString)
            {
                loadZoneHook!.Original(layoutManager, id, bg, (byte*)charPtr, territoryType, layerFilterKey, type, festivals, cfcId);
                return;
            }
        }
        
        loadZoneHook!.Original(layoutManager, id, bg, bgNoExtension, territoryType, layerFilterKey, type, festivals, cfcId);
    }
    
    private static void ToggleCulling(bool enabled)
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        var config = GraphicsConfig.Instance();
        if (config == null) return;

        Plugin.Log.Verbose($"Setting GraphicsConfig->IsInside to {enabled}");
        ((GraphicsConfigEx*)config)->IsInside = enabled; // just tell the game we're outside duh

        Plugin.Framework.Run(Scene.RedrawObjects); // redraw objects in case they were already culled
    }
    
    public void Dispose()
    {
        loadZoneHook?.Dispose();
        Enabled = false;
        
        Plugin.Framework.Run(() => ToggleCulling(true));
        Plugin.Log.Debug("Disposed Fake Outside fix.");
    }
}
