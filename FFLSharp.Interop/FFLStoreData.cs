using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLStoreData
    {
        [NativeTypeName("__AnonymousRecord_FFLStandard_L13_C5")]
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

        public ref uint value32
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->value32[0];
                }
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
}
