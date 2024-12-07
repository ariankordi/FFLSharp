namespace FFLSharp.Interop
{
    public unsafe partial struct FFLCharModelSource
    {
        public FFLDataSource dataSource;

        [NativeTypeName("const void *")]
        public void* pBuffer;

        [NativeTypeName("u16")]
        public ushort index;
    }
}
