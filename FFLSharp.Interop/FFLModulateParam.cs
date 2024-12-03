namespace FFLSharp.Interop;

public unsafe partial struct FFLModulateParam
{
    public FFLModulateMode mode;

    public FFLModulateType type;

    [NativeTypeName("const FFLColor *")]
    public FFLColor* pColorR;

    [NativeTypeName("const FFLColor *")]
    public FFLColor* pColorG;

    [NativeTypeName("const FFLColor *")]
    public FFLColor* pColorB;

    [NativeTypeName("const FFLTexture *")]
    public void* pTexture2D;
}
