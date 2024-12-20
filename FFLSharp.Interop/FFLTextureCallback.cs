using System;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLTextureCallback
    {
        public void* pObj;

        [NativeTypeName("bool")]
        public byte useOriginalTileMode;

        [NativeTypeName("u8[3]")]
        public fixed byte _padding[3];

        [NativeTypeName("void (*)(void *, const FFLTextureInfo *, FFLTexture *)")]
        public IntPtr pCreateFunc;

        [NativeTypeName("void (*)(void *, FFLTexture *)")]
        public IntPtr pDeleteFunc;
    }
}
