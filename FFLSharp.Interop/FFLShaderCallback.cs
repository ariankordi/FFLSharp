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

        [NativeTypeName("void (*)(void *, bool, FFLRIOCompareFunc, f32)")]
        public IntPtr pApplyAlphaTestFunc;

        [NativeTypeName("void (*)(void *, const FFLDrawParam *)")]
        public IntPtr pDrawFunc;

        [NativeTypeName("void (*)(void *, const FFLRIOBaseMtx44f *)")]
        public IntPtr pSetMatrixFunc;
    }
}
