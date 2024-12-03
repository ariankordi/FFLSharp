namespace FFLSharp.Interop;

public unsafe partial struct FFLShaderCallback
{
    public void* pObj;

    [NativeTypeName("void (*)(void *, bool, FFLRIOCompareFunc, f32)")]
    public delegate* unmanaged[Cdecl]<void*, byte, uint, float, void> pApplyAlphaTestFunc;

    [NativeTypeName("void (*)(void *, const FFLDrawParam *)")]
    public delegate* unmanaged[Cdecl]<void*, FFLDrawParam*, void> pDrawFunc;

    [NativeTypeName("void (*)(void *, const FFLRIOBaseMtx44f *)")]
    public delegate* unmanaged[Cdecl]<void*, FFLRIOBaseMtx44f*, void> pSetMatrixFunc;
}
