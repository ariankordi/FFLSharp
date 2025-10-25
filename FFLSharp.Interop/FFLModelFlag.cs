using static FFLSharp.Interop.FFLModelType;

namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLModelFlag : uint
    {
        FFL_MODEL_FLAG_NORMAL = unchecked(1 << (int)(FFL_MODEL_TYPE_NORMAL)),
        FFL_MODEL_FLAG_HAT = unchecked(1 << (int)(FFL_MODEL_TYPE_HAT)),
        FFL_MODEL_FLAG_FACE_ONLY = unchecked(1 << (int)(FFL_MODEL_TYPE_FACE_ONLY)),
        FFL_MODEL_FLAG_FLATTEN_NOSE = 1 << 3,
        FFL_MODEL_FLAG_NEW_EXPRESSIONS = 1 << 4,
        FFL_MODEL_FLAG_AFL_MODE = 1 << 6,
    }
}
