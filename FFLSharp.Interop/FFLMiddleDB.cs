using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLMiddleDB
    {
        [NativeTypeName("__AnonymousRecord_FFLMiddleDB_L19_C5")]
        public _Anonymous_e__Union Anonymous;

        public ref byte data
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->data[0];
                }
            }
        }

        public ref uint data32
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->data32[0];
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("u8[24]")]
            public fixed byte data[24];

            [FieldOffset(0)]
            [NativeTypeName("u32[6]")]
            public fixed uint data32[6];
        }
    }
}
