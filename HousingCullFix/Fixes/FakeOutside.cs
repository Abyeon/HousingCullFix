using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using HousingCullFix.Utils;
using InteropGenerator.Runtime;

namespace HousingCullFix.Fixes;

public unsafe class FakeOutside : IFix
{
    public string Name { get; init; } = "Fake Outside (Recommended)";

    public string Description { get; init; } = "Flips a \"IsInside\" boolean the game checks before running their new culling algorithm.\n" +
                                               "This has the side benefit of re-enabling void lighting.";
    
    public bool Enabled { get; set; }
    
    private delegate void LoadZoneDelegate(LayoutManager* layoutManager, uint id, CStringPointer bg, CStringPointer bgNoExtension, uint territoryType, uint layerFilterKey, int type, GameMain.Festival[] festivals, uint cfcId);

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
    }

    public void Disable()
    {
        loadZoneHook?.Disable();
        Plugin.Framework.Run(() => ToggleCulling(true));
        Enabled = false;
    }
    
    public void LoadZoneDetour(LayoutManager* layoutManager, uint id, CStringPointer bg, CStringPointer bgNoExtension, uint territoryType, uint layerFilterKey, int type, GameMain.Festival[] festivals, uint cfcId)
    {
        Plugin.Log.Verbose($"Loading: {bg}");
        
        // if (bgNoExtension.ToString().Contains("/ind/"))
        // {
        //     // Just to note: replacing bgNoExtension /ind/ or just removing it cancels the logic in this function that:
        //     // 1) Changes GraphicsConfig->IsInside (0x6A) to true
        //     // 2) Inits the new culling object found at ffxiv_dx11.exe+28F7190
        //     // var newString = bgNoExtension.ToString().Replace("/ind/", "/hehe/");
        //     var newString = "/ind/";
        //     fixed (char* charPtr = newString)//
        //     {
        //         loadZoneHook!.Original(layoutManager, id, bg, (byte*)charPtr, territoryType, layerFilterKey, type, festivals, cfcId);
        //         //GraphicsConfig.Instance()->IsIndoor = true;
        //         LayoutWorld.Instance()->ActiveLayout->HousingType = 2;
        //         return;
        //     }
        // }
        
        loadZoneHook!.Original(layoutManager, id, bg, bgNoExtension, territoryType, layerFilterKey, type, festivals, cfcId);

        if (bgNoExtension.ToString().Contains("/ind/"))
        {
            ToggleCulling(false);
        }
    }
    
    private static void ToggleCulling(bool enabled)
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        var config = GraphicsConfig.Instance();
        if (config == null) return;

        Plugin.Log.Verbose($"Setting GraphicsConfig->IsInside to {enabled}");
        config->IsIndoor = enabled; // just tell the game we're outside duh

        Plugin.Framework.Run(Scene.RedrawObjects); // redraw objects in case they were already culled
    }
    
    public void Dispose()
    {
        loadZoneHook?.Dispose();
        Enabled = false;
        
        Plugin.Framework.Run(() => ToggleCulling(true));
    }
}
