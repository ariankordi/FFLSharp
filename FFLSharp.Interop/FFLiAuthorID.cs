using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

public unsafe partial struct FFLiAuthorID
{
    [NativeTypeName("__AnonymousRecord_FFLiAuthorID_L10_C5")]
    public _Anonymous_e__Union Anonymous;

    public Span<byte> data
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.data[0], 8);
        }
    }

    public Span<ushort> value16
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.value16[0], 4);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct _Anonymous_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("u8[8]")]
        public fixed byte data[8];

        [FieldOffset(0)]
        [NativeTypeName("u16[4]")]
        public fixed ushort value16[4];
    }
}
