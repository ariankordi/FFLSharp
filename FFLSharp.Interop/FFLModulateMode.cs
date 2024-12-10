namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLModulateMode : uint
    {
        FFL_MODULATE_MODE_CONSTANT = 0,
        FFL_MODULATE_MODE_TEXTURE_DIRECT = 1,
        FFL_MODULATE_MODE_RGB_LAYERED = 2,
        FFL_MODULATE_MODE_ALPHA = 3,
        FFL_MODULATE_MODE_LUMINANCE_ALPHA = 4,
        FFL_MODULATE_MODE_ALPHA_OPA = 5,
    }
}
