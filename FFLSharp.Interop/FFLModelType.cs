namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLModelType : uint
    {
        FFL_MODEL_TYPE_NORMAL = 0,
        FFL_MODEL_TYPE_HAT = 1,
        FFL_MODEL_TYPE_FACE_ONLY = 2,
        FFL_MODEL_TYPE_MAX = 3,
    }
}
