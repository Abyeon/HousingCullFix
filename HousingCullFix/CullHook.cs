using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using HousingCullFix.Structs;

namespace HousingCullFix;

public unsafe class CullHook : IDisposable
{
    //char __fastcall sub_1404536B0(__int64 a1, int a2, unsigned int a3, float a4)
    private delegate byte CullDelegate(BgObject* a1, int a2, uint a3, float a4);
    
    [Signature("40 55 56 57 41 55 41 56 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 0F 29 B4 24", DetourName = nameof(DetourCull))]
    private Hook<CullDelegate>? cullHook;
    
    public CullHook()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
        cullHook?.Enable();

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
    
    public void Dispose()
    {
        cullHook?.Dispose();
    }
}
