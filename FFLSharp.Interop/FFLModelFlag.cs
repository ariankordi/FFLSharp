using static FFLSharp.Interop.FFLModelType;

namespace FFLSharp.Interop;

public enum FFLModelFlag
{
    FFL_MODEL_FLAG_NORMAL = 1 << (int)(FFL_MODEL_TYPE_NORMAL),
    FFL_MODEL_FLAG_HAT = 1 << (int)(FFL_MODEL_TYPE_HAT),
    FFL_MODEL_FLAG_FACE_ONLY = 1 << (int)(FFL_MODEL_TYPE_FACE_ONLY),
    FFL_MODEL_FLAG_FLATTEN_NOSE = 1 << 3,
}
