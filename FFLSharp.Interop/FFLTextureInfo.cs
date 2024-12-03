namespace FFLSharp.Interop;

public unsafe partial struct FFLTextureInfo
{
    [NativeTypeName("u32")]
    public uint width;

    [NativeTypeName("u32")]
    public uint height;

    public FFLiTextureFormat format;

    [NativeTypeName("u32")]
    public uint size;

    [NativeTypeName("bool")]
    public byte isGX2Tiled;

    [NativeTypeName("u32")]
    public uint numMips;

    [NativeTypeName("const void *")]
    public void* imagePtr;

    [NativeTypeName("const void *")]
    public void* mipPtr;
}
