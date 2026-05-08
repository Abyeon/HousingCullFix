using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace HousingCullFix.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public struct CullObject
{
    [FieldOffset(0x00)] public Vector3 Position;
    [FieldOffset(0x10)] public Vector3 Offset;
    [FieldOffset(0x20)] public int Unk0;
    [FieldOffset(0x24)] public float Distance; // and or visibility flag
}

[InlineArray(600)]
public struct CullObjectArray
{
    private CullObject element0;
}

[StructLayout(LayoutKind.Explicit, Size = 0x907C)]
public unsafe struct AreaCullingManager
{
    [FieldOffset(0x0030)] public CullObjectArray CullObjects;

    [FieldOffset(0x907A)] public byte Fade;
    [FieldOffset(0x907B)] public bool Unk907B;
    
    public static AreaCullingManager* Instance()
    {
        return *(AreaCullingManager**)Plugin.SigScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 03 C9");
    }
}
