using System;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLShaderCallback
    {
        public void* pObj;

        [NativeTypeName("bool")]
        public byte facelineColorIsTransparent;

        [NativeTypeName("u8[3]")]
        public fixed byte _padding[3];

        [NativeTypeName("void *[1]")]
        public __padding1_e__FixedBuffer _padding1;

        [NativeTypeName("void (*)(void *, const FFLDrawParam *)")]
        public IntPtr pDrawFunc;

        [NativeTypeName("void (*)(void *, const float *)")]
        public IntPtr pSetMatrixFunc;

        public unsafe partial struct __padding1_e__FixedBuffer
        {
            public void* e0;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
