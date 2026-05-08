using System;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace HousingCullFix.Utils;

public unsafe class HouseFunctions
{
    private delegate nint SetInteriorLightDelegate(IndoorTerritory* indoor, nint inverse, nint a3);

    [Signature("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 0F B6 FA 48 8B D9 40 80 FF")]
    private readonly SetInteriorLightDelegate? setInteriorLight = null;

    public HouseFunctions()
    {
        Plugin.GameInteropProvider.InitializeFromAttributes(this);
    }

    public void SetInteriorLight(uint target)
    {
        if (setInteriorLight == null)
            throw new InvalidOperationException("SetInteriorLight sig wasn't found!");

        var man = HousingManager.Instance();
        if (!man->IsInside())
        {
            Plugin.Log.Warning("Cannot set interior light while not inside.");
            return;
        }

        var indoor = man->IndoorTerritory;
        if (indoor == null)
        {
            throw new NullReferenceException("Indoor territory was null!");
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(target, 5, "Target was higher than allowed!");

        var inverse = (nint)(5 - target);
        
        
        setInteriorLight(indoor, inverse, 0);
    }
}
