namespace FFLSharp.Interop
{
    public unsafe partial struct FFLPrimitiveParam
    {
        [NativeTypeName("FFLRIOPrimitiveMode")]
        public uint primitiveType;

        [NativeTypeName("u32")]
        public uint indexCount;

        [NativeTypeName("u32")]
        public uint _8;

        public void* pIndexBuffer;
    }
}
