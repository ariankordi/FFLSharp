namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLTextureFormat : uint
    {
        FFL_TEXTURE_FORMAT_R8_UNORM = 0,
        FFL_TEXTURE_FORMAT_R8_G8_UNORM = 1,
        FFL_TEXTURE_FORMAT_R8_G8_B8_A8_UNORM = 2,
        FFL_TEXTURE_FORMAT_MAX = 3,
    }
}
