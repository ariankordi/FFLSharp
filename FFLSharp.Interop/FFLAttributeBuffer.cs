namespace FFLSharp.Interop
{
    public unsafe partial struct FFLAttributeBuffer
    {
        [NativeTypeName("u32")]
        public uint size;

        [NativeTypeName("u32")]
        public uint stride;

        public void* ptr;
    }
}
