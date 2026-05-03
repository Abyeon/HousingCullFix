using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using HousingCullFix.Structs;
using InteropGenerator.Runtime;

namespace HousingCullFix.Fixes;

public unsafe class FakeOutside : IFix
{
    public string Name { get; init; } = "Fake Outside (Recommended)";

    public string Description { get; init; } = "Flips a \"IsInside\" boolean the game checks before running their new culling algorithm.\n" +
                                               "This has the side benefit of re-enabling void lighting.";
    
    public bool Enabled { get; set; }
    
    private delegate void LoadIndoorsDelegate(IntPtr a1, int a2, CStringPointer a3, IntPtr a4, int a5, int a6, int a7, IntPtr a8, int a9);

    [Signature("40 53 48 83 EC ?? 8B 44 24 ?? 48 8B D9 89 41", DetourName = nameof(LoadIndoorsDetour))]
    private readonly Hook<LoadIndoorsDelegate>? loadIndoorsHook = null!;

    public FakeOutside()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }
    
    public void Enable()
    {
        loadIndoorsHook?.Enable();
        Plugin.Framework.Run(() => ToggleCulling(false));
        Enabled = true;

        Plugin.Log.Debug("Enabled Fake Outside fix.");
    }

    public void Disable()
    {
        loadIndoorsHook?.Disable();
        Plugin.Framework.Run(() => ToggleCulling(true));
        Enabled = false;
        
        Plugin.Log.Debug("Disabled Fake Outside fix.");
    }
    
    public void LoadIndoorsDetour( IntPtr a1, int a2, CStringPointer level, IntPtr a4, int a5, int a6, int a7, IntPtr a8, int a9)
    {
        loadIndoorsHook!.Original(a1, a2, level, a4, a5, a6, a7, a8, a9);
        
        Plugin.Log.Verbose($"Loading: {level}");
        if (!level.ToString().Contains("/ind/")) return;
        
        ToggleCulling(false);
    }
    
    private static void ToggleCulling(bool enabled)
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        var config = GraphicsConfig.Instance();
        if (config == null) return;

        ((GraphicsConfigEx*)config)->IsInside = enabled; // just tell the game we're outside duh

        Plugin.Framework.Run(Utils.RedrawObjects); // redraw objects in case they were already culled
    }
    
    public void Dispose()
    {
        loadIndoorsHook?.Dispose();
        Enabled = false;
        
        Plugin.Framework.Run(() => ToggleCulling(true));
        Plugin.Log.Debug("Disposed Fake Outside fix.");
    }
}
