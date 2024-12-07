using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiAuthorID
    {
        [NativeTypeName("__AnonymousRecord_FFLiAuthorID_L10_C5")]
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

        public ref ushort value16
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->value16[0];
                }
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
}
