using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiMiddleDB
    {
        [NativeTypeName("u32")]
        public uint m_Magic;

        public FFLMiddleDBType m_Type;

        public void* m_pMiiDataOfficial;

        [NativeTypeName("u16")]
        public ushort m_Size;

        [NativeTypeName("u16")]
        public ushort m_StoredSize;

        [NativeTypeName("__AnonymousRecord_FFLiMiddleDB_L147_C5")]
        public _Anonymous_e__Union Anonymous;

        public ref byte m_ParamData
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->m_ParamData[0];
                }
            }
        }

        public ref byte m_HiddenParam
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->m_HiddenParam[0];
                }
            }
        }

        public ref byte m_RandomParam
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->m_RandomParam[0];
                }
            }
        }

        public ref byte m_NetParam
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->m_NetParam[0];
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("u8[4]")]
            public fixed byte m_ParamData[4];

            [FieldOffset(0)]
            [NativeTypeName("char[4]")]
            public fixed byte m_HiddenParam[4];

            [FieldOffset(0)]
            [NativeTypeName("char[4]")]
            public fixed byte m_RandomParam[4];

            [FieldOffset(0)]
            [NativeTypeName("u8[4]")]
            public fixed byte m_NetParam[4];
        }
    }
}
