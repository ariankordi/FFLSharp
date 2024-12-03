using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

public partial struct FFLStoreData
{
}

public unsafe partial struct FFLStoreData
{
    [NativeTypeName("__AnonymousRecord_FFLStandard_L13_C5")]
    public _Anonymous_e__Union Anonymous;

    public Span<byte> data
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.data[0], 96);
        }
    }

    public Span<uint> value32
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.value32[0], 24);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct _Anonymous_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("u8[96]")]
        public fixed byte data[96];

        [FieldOffset(0)]
        [NativeTypeName("u32[24]")]
        public fixed uint value32[24];
    }
}
