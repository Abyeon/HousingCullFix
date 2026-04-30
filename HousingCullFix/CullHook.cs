using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using HousingCullFix.Structs;

namespace HousingCullFix;

public unsafe class CullHook : IDisposable
{
    //char __fastcall sub_1404536B0(__int64 a1, int a2, unsigned int a3, float a4)
    private delegate byte CullDelegate(BgObject* a1, int a2, uint a3, float a4);
    
    [Signature("40 55 56 57 41 55 41 56 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 0F 29 B4 24", DetourName = nameof(DetourCull))]
    private Hook<CullDelegate>? cullHook;

    // private delegate GraphicsConfig* CullConeDelegate(IntPtr a1);
    //
    // [Signature("48 89 5C 24 ?? 56 48 83 EC ?? 33 F6 48 8B D9 89 B1 ?? ?? ?? ?? 48 8B 05", DetourName = nameof(DetourCullCone))]
    // private Hook<CullConeDelegate>? cullCone;
    
    public CullHook()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        cullHook?.Enable();
        // cullCone?.Enable();

        Plugin.Log.Debug("Enabled hook!");
    }

    public byte DetourCull(BgObject* a1, int a2, uint a3, float a4)
    {
        //Plugin.Log.Debug($"{a1}, {a2}, {a3}, {a4}");
        
        try
        {
            var man = HousingManager.Instance();
            if (man != null && man->IsInside())
            {
                var ex = (BgObjectEx*)a1;
                ex->Visibility = 0xFFFF;
                return cullHook!.Original(a1, a2, a3, a4);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"{e}");
        }
        
        return cullHook!.Original(a1, a2, a3, a4);
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

    private void FixObjects()
    {
        var man = HousingManager.Instance();
        if (man == null && !man->IsInside()) return;

        var furnitureMan = man->GetFurnitureManager();
        
        foreach (ref var ptr in furnitureMan->ObjectManager.ObjectArray.Objects)
        {
            var gameObject = ptr.Value;
            if (gameObject == null) continue;
            
            gameObject->DisableDraw();
        }
    }
    
    public void Dispose()
    {
        cullHook?.Dispose();
        // cullCone?.Dispose();
        
        FixObjects();
    }
}
