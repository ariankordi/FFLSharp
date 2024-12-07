using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLCharModel
    {
        [NativeTypeName("__AnonymousRecord_FFLCharModel_L31_C5")]
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
            [NativeTypeName("u8[3064]")]
            public fixed byte data[3064];

            [FieldOffset(0)]
            [NativeTypeName("u32[766]")]
            public fixed uint data32[766];
        }
    }
}
