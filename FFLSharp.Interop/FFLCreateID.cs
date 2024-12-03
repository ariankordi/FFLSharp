using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

public unsafe partial struct FFLCreateID
{
    [NativeTypeName("__AnonymousRecord_FFLCreateID_L14_C5")]
    public _Anonymous_e__Union Anonymous;

    public Span<byte> data
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.data[0], 10);
        }
    }

    public Span<ushort> value16
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.value16[0], 5);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct _Anonymous_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("u8[10]")]
        public fixed byte data[10];

        [FieldOffset(0)]
        [NativeTypeName("u16[5]")]
        public fixed ushort value16[5];
    }
}
