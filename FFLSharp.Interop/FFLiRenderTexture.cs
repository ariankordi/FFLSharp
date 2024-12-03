namespace FFLSharp.Interop;

public unsafe partial struct FFLiRenderTexture
{
    [NativeTypeName("FFLTexture *")]
    public void* pTexture2D;

    public void* pRenderBuffer;

    public void* pColorTarget;

    public void* pDepthTarget;
}
