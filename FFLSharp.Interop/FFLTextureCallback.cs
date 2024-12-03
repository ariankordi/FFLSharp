namespace FFLSharp.Interop;

public unsafe partial struct FFLTextureCallback
{
    public void* pObj;

    [NativeTypeName("void (*)(void *, const FFLTextureInfo *, FFLTexture *)")]
    public delegate* unmanaged[Cdecl]<void*, FFLTextureInfo*, void*, void> pCreateFunc;

    [NativeTypeName("void (*)(void *, FFLTexture *)")]
    public delegate* unmanaged[Cdecl]<void*, void*, void> pDeleteFunc;
}

public partial struct FFLTextureCallback
{
}

public partial struct FFLTextureCallback
{
}
