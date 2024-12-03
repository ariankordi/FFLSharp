using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct FFLAllExpressionFlag
{
    [FieldOffset(0)]
    [NativeTypeName("__AnonymousRecord_FFLExpressionFlag_L17_C5")]
    public __0_e__Struct _0;

    [FieldOffset(0)]
    [NativeTypeName("u32[3]")]
    public fixed uint flags[3];

    public partial struct __0_e__Struct
    {
        [NativeTypeName("u32")]
        public uint low;

        [NativeTypeName("u32")]
        public uint mid;

        [NativeTypeName("u32")]
        public uint high;
    }
}
