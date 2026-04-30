using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using HousingCullFix.Structs;
using InteropGenerator.Runtime;

namespace HousingCullFix;

public unsafe class CullHook : IDisposable
{
    //char __fastcall sub_1404536B0(__int64 a1, int a2, unsigned int a3, float a4)
    // private delegate byte CullDelegate(BgObject* a1, int a2, uint a3, float a4);
    //
    // [Signature("40 55 56 57 41 55 41 56 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 0F 29 B4 24", DetourName = nameof(CullDetour))]
    // private Hook<CullDelegate>? cullHook;
    
    private delegate void LoadIndoorsDelegate(IntPtr a1, int a2, CStringPointer a3, IntPtr a4, int a5, int a6, int a7, IntPtr a8, int a9);

    [Signature("40 53 48 83 EC ?? 8B 44 24 ?? 48 8B D9 89 41", DetourName = nameof(LoadIndoorsDetour))]
    private Hook<LoadIndoorsDelegate>? loadIndoorsHook;
    
    // private delegate GraphicsConfig* CullConeDelegate(IntPtr a1);
    //
    // [Signature("48 89 5C 24 ?? 56 48 83 EC ?? 33 F6 48 8B D9 89 B1 ?? ?? ?? ?? 48 8B 05", DetourName = nameof(DetourCullCone))]
    // private Hook<CullConeDelegate>? cullCone;
    
    public CullHook()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        // cullHook?.Enable();
        loadIndoorsHook?.Enable();
        // cullCone?.Enable();
        
        DisableCull();

        Plugin.Log.Verbose("Enabled CullHook!");
    }

    private static void DisableCull()
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        var config = (GraphicsConfigEx*)GraphicsConfig.Instance();
        if (config == null) return;

        config->IsInside = false; // just tell the game we're outside duh

        Plugin.Framework.Run(RedrawObjects); // redraw objects in case they were already culled
    }

    private static void EnableCull()
    {
        var man = HousingManager.Instance();
        if (man == null || !man->IsInside()) return;
        
        var config = (GraphicsConfigEx*)GraphicsConfig.Instance();
        if (config == null) return;

        config->IsInside = true;
    }
    
    private static void RedrawObjects()
    {
        var man = HousingManager.Instance();
        if (man == null) return;

        var furnitureMan = man->GetFurnitureManager();
        
        foreach (ref var ptr in furnitureMan->ObjectManager.ObjectArray.Objects)
        {
            var gameObject = ptr.Value;
            if (gameObject == null) continue;
            
            gameObject->DisableDraw();
            
            Plugin.Log.Verbose($"Redrawing {gameObject->NameString}");
        }
    }

    // public byte CullDetour(BgObject* a1, int a2, uint a3, float a4)
    // {
    //     //Plugin.Log.Debug($"{a1}, {a2}, {a3}, {a4}");
    //     
    //     try
    //     {
    //         var man = HousingManager.Instance();
    //         if (man != null && (man->IsInside() || man->IsOutside()))
    //         {
    //             var ex = (BgObjectEx*)a1;
    //             ex->Visibility = 0xFFFF;
    //             return cullHook!.Original(a1, a2, a3, a4);
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Plugin.Log.Error($"{e}");
    //     }
    //     
    //     return cullHook!.Original(a1, a2, a3, a4);
    // }

    public void LoadIndoorsDetour( IntPtr a1, int a2, CStringPointer a3, IntPtr a4, int a5, int a6, int a7, IntPtr a8, int a9)
    {
        loadIndoorsHook!.Original(a1, a2, a3, a4, a5, a6, a7, a8, a9);
        
        // Plugin.Log.Debug($"{a3}");
        
        DisableCull();
    }
    
    //
    // public GraphicsConfig* DetourCullCone(IntPtr a1)
    // {
    //     try
    //     {
    //         Plugin.Log.Debug($"{a1:X}");
    //     }
    //     catch (Exception e)
    //     {
    //         Plugin.Log.Error($"{e}");
    //     }
    //     
    //     return cullCone!.Original(a1);
    // }
    
    public void Dispose()
    {
        // cullHook?.Dispose();
        loadIndoorsHook?.Dispose();
        
        Plugin.Framework.Run(EnableCull);
        Plugin.Framework.Run(RedrawObjects);
        Plugin.Log.Verbose("CullHook disposed");
    }
}
