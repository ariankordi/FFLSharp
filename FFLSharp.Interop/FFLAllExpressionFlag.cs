using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct FFLAllExpressionFlag
    {
        [FieldOffset(0)]
        [NativeTypeName("__AnonymousRecord_FFLExpressionFlag_L14_C5")]
        public _flag_e__Struct flag;

        [FieldOffset(0)]
        [NativeTypeName("u32[3]")]
        public fixed uint flags[3];

        public partial struct _flag_e__Struct
        {
            [NativeTypeName("u32")]
            public uint low;

            [NativeTypeName("u32")]
            public uint mid;

            [NativeTypeName("u32")]
            public uint high;
        }
    }
}
