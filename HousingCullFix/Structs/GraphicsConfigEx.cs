using System.Runtime.InteropServices;

namespace HousingCullFix.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0xD8)]
public struct GraphicsConfigEx
{
    [FieldOffset(0x6A)] public bool IsInside;
}
