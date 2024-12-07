namespace FFLSharp.Interop
{
    public unsafe partial struct FFLRIOTexture2D
    {
        [NativeTypeName("FFLRIONativeTexture2D")]
        public fixed byte mTextureInner[128];

        [NativeTypeName("FFLRIONativeTexture2DHandle")]
        public uint mHandle;

        [NativeTypeName("bool")]
        public byte mSelfAllocated;
    }
}
