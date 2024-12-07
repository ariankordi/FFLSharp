namespace FFLSharp.Interop
{
    public unsafe partial struct FFLResourceDesc
    {
        [NativeTypeName("void *[2]")]
        public _pData_e__FixedBuffer pData;

        [NativeTypeName("u32[2]")]
        public fixed uint size[2];

        public unsafe partial struct _pData_e__FixedBuffer
        {
            public void* e0;
            public void* e1;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
