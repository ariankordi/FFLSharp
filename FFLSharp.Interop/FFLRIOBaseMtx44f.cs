using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLRIOBaseMtx44f
    {
        [NativeTypeName("__AnonymousRecord_FFLRIOInterop_L16_C2")]
        public _Anonymous_e__Union Anonymous;

        public ref float m
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->m[0];
                }
            }
        }

        public ref float a
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->a[0];
                }
            }
        }

        public ref _Anonymous_e__Union._v_e__FixedBuffer v
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->v;
                }
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

                public unsafe ref FFLVec4 this[int index]
                {
                    get
                    {
                        fixed (FFLVec4* pThis = &e0)
                        {
                            return ref pThis[index];
                        }
                    }
                }
            }
        }
    }
}
