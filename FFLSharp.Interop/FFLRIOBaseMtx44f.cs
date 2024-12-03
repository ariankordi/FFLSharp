using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

public unsafe partial struct FFLRIOBaseMtx44f
{
    [NativeTypeName("__AnonymousRecord_FFLRIOInterop_L16_C2")]
    public _Anonymous_e__Union Anonymous;

    public Span<float> m
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.m[0], 4);
        }
    }

    public Span<float> a
    {
        get
        {
            return MemoryMarshal.CreateSpan(ref Anonymous.a[0], 16);
        }
    }

    public Span<FFLVec4> v
    {
        get
        {
            return Anonymous.v.AsSpan();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct _Anonymous_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("f32[4][4]")]
        public fixed float m[4 * 4];

        [FieldOffset(0)]
        [NativeTypeName("f32[16]")]
        public fixed float a[16];

        [FieldOffset(0)]
        [NativeTypeName("FFLVec4[4]")]
        public _v_e__FixedBuffer v;

        public partial struct _v_e__FixedBuffer
        {
            public FFLVec4 e0;
            public FFLVec4 e1;
            public FFLVec4 e2;
            public FFLVec4 e3;

            public ref FFLVec4 this[int index]
            {
                get
                {
                    return ref AsSpan()[index];
                }
            }

            public Span<FFLVec4> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 4);
        }
    }
}
