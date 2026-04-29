using System;
using System.Runtime.InteropServices;

namespace HousingCullFix.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0xE0)]
public unsafe struct BgObjectEx
{
    [FieldOffset(0xD4)] public ushort Visibility;
}
