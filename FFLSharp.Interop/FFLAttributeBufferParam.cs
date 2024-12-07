namespace FFLSharp.Interop
{
    public partial struct FFLAttributeBufferParam
    {
        [NativeTypeName("FFLAttributeBuffer[5]")]
        public _attributeBuffers_e__FixedBuffer attributeBuffers;

        public partial struct _attributeBuffers_e__FixedBuffer
        {
            public FFLAttributeBuffer e0;
            public FFLAttributeBuffer e1;
            public FFLAttributeBuffer e2;
            public FFLAttributeBuffer e3;
            public FFLAttributeBuffer e4;

            public unsafe ref FFLAttributeBuffer this[int index]
            {
                get
                {
                    fixed (FFLAttributeBuffer* pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
