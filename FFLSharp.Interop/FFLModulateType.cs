namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLModulateType : uint
    {
        FFL_MODULATE_TYPE_SHAPE_FACELINE = 0,
        FFL_MODULATE_TYPE_SHAPE_BEARD = 1,
        FFL_MODULATE_TYPE_SHAPE_NOSE = 2,
        FFL_MODULATE_TYPE_SHAPE_FOREHEAD = 3,
        FFL_MODULATE_TYPE_SHAPE_HAIR = 4,
        FFL_MODULATE_TYPE_SHAPE_CAP = 5,
        FFL_MODULATE_TYPE_SHAPE_MASK = 6,
        FFL_MODULATE_TYPE_SHAPE_NOSELINE = 7,
        FFL_MODULATE_TYPE_SHAPE_GLASS = 8,
        FFL_MODULATE_TYPE_MUSTACHE = 9,
        FFL_MODULATE_TYPE_MOUTH = 10,
        FFL_MODULATE_TYPE_EYEBROW = 11,
        FFL_MODULATE_TYPE_EYE = 12,
        FFL_MODULATE_TYPE_MOLE = 13,
        FFL_MODULATE_TYPE_FACE_MAKE = 14,
        FFL_MODULATE_TYPE_FACE_LINE = 15,
        FFL_MODULATE_TYPE_FACE_BEARD = 16,
        FFL_MODULATE_TYPE_FILL = 17,
        FFL_MODULATE_TYPE_SHAPE_MAX = FFL_MODULATE_TYPE_SHAPE_GLASS + 1,
    }
}
