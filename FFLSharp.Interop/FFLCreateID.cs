using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLCreateID
    {
        [NativeTypeName("__AnonymousRecord_FFLCreateID_L14_C5")]
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
            [NativeTypeName("u8[10]")]
            public fixed byte data[10];

            [FieldOffset(0)]
            [NativeTypeName("u16[5]")]
            public fixed ushort value16[5];
        }
    }
}
