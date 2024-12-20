namespace FFLSharp.Interop
{
    public unsafe partial struct FFLTextureInfo
    {
        [NativeTypeName("u16")]
        public ushort width;

        [NativeTypeName("u16")]
        public ushort height;

        [NativeTypeName("u8")]
        public byte mipCount;

        [NativeTypeName("u8")]
        public byte format;

        [NativeTypeName("bool")]
        public byte isGX2Tiled;

        [NativeTypeName("u8[1]")]
        public fixed byte _padding[1];

        [NativeTypeName("u32")]
        public uint imageSize;

        public void* imagePtr;

        [NativeTypeName("u32")]
        public uint mipSize;

        public void* mipPtr;

        [NativeTypeName("u32[13]")]
        public fixed uint mipLevelOffset[13];
    }
}
