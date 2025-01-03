namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLAttributeBufferType : uint
    {
        FFL_ATTRIBUTE_BUFFER_TYPE_POSITION = 0,
        FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD = 1,
        FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL = 2,
        FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT = 3,
        FFL_ATTRIBUTE_BUFFER_TYPE_COLOR = 4,
        FFL_ATTRIBUTE_BUFFER_TYPE_MAX = 5,
    }
}
