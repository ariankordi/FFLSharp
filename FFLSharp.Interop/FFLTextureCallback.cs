using System;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLTextureCallback
    {
        public void* pObj;

        [NativeTypeName("void (*)(void *, const FFLTextureInfo *, FFLTexture *)")]
        public IntPtr pCreateFunc;

        [NativeTypeName("void (*)(void *, FFLTexture *)")]
        public IntPtr pDeleteFunc;
    }
}
