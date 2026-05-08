using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using HousingCullFix.Structs;
using HousingCullFix.Utils;

namespace HousingCullFix.Fixes;

public unsafe class HookAndChange : IFix
{
    public string Name { get; init; } = "Hook and Change";

    public string Description { get; init; } = "Hooks the culling function and changes the Draw Object's visibility value.\n" +
                                               "This does not fix light culling, and may have other issues.";
    
    public bool Enabled { get; set; }
    
    //char __fastcall sub_1404536B0(__int64 a1, int a2, unsigned int a3, float a4)
    private delegate byte CullDelegate(BgObject* a1, int a2, uint a3, float a4);
    
    [Signature("40 55 56 57 41 55 41 56 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 0F 29 B4 24", DetourName = nameof(CullDetour))]
    private readonly Hook<CullDelegate>? cullHook = null!;

    public HookAndChange()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }
    
    public void Enable()
    {
        cullHook?.Enable();
        Enabled = true;
    }

    public void Disable()
    {
        cullHook?.Disable();
        Enabled = false;
        Plugin.Framework.Run(Scene.RedrawObjects);
    }
    
    public byte CullDetour(BgObject* a1, int a2, uint a3, float a4)
    {
        try
        {
            var man = HousingManager.Instance();
            if (man != null && (man->IsInside() || man->IsOutside()))
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
        Enabled = false;
        Plugin.Framework.Run(Scene.RedrawObjects);
    }
}
