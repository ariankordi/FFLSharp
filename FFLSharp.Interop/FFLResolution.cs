namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLResolution : uint
    {
        FFL_RESOLUTION_MASK = 0x3fffffff,
        FFL_RESOLUTION_MIP_MAP_ENABLE_MASK = 1 << 30,
        FFL_RESOLUTION_TEX_128 = 128,
        FFL_RESOLUTION_TEX_192 = 192,
        FFL_RESOLUTION_TEX_256 = 256,
        FFL_RESOLUTION_TEX_384 = 384,
        FFL_RESOLUTION_TEX_512 = 512,
        FFL_RESOLUTION_TEX_768 = 768,
        FFL_RESOLUTION_TEX_1024 = 1024,
    }
}
