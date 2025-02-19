namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLCullMode : uint
    {
        FFL_CULL_MODE_NONE = 0,
        FFL_CULL_MODE_BACK = 1,
        FFL_CULL_MODE_FRONT = 2,
        FFL_CULL_MODE_MAX = 3,
    }
}
