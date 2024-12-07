namespace FFLSharp.Interop
{
    public partial struct FFLiRawMaskDrawParam
    {
        [NativeTypeName("FFLiRawMaskPartsDrawParam[2]")]
        public _drawParamRawMaskPartsEye_e__FixedBuffer drawParamRawMaskPartsEye;

        [NativeTypeName("FFLiRawMaskPartsDrawParam[2]")]
        public _drawParamRawMaskPartsEyebrow_e__FixedBuffer drawParamRawMaskPartsEyebrow;

        [NativeTypeName("FFLiRawMaskPartsDrawParam")]
        public FFLDrawParam drawParamRawMaskPartsMouth;

        [NativeTypeName("FFLiRawMaskPartsDrawParam[2]")]
        public _drawParamRawMaskPartsMustache_e__FixedBuffer drawParamRawMaskPartsMustache;

        [NativeTypeName("FFLiRawMaskPartsDrawParam")]
        public FFLDrawParam drawParamRawMaskPartsMole;

        [NativeTypeName("FFLiRawMaskPartsDrawParam")]
        public FFLDrawParam drawParamRawMaskPartsFill;

        public partial struct _drawParamRawMaskPartsEye_e__FixedBuffer
        {
            public FFLDrawParam e0;
            public FFLDrawParam e1;

            public unsafe ref FFLDrawParam this[int index]
            {
                get
                {
                    fixed (FFLDrawParam* pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }

        public partial struct _drawParamRawMaskPartsEyebrow_e__FixedBuffer
        {
            public FFLDrawParam e0;
            public FFLDrawParam e1;

            public unsafe ref FFLDrawParam this[int index]
            {
                get
                {
                    fixed (FFLDrawParam* pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }

        public partial struct _drawParamRawMaskPartsMustache_e__FixedBuffer
        {
            public FFLDrawParam e0;
            public FFLDrawParam e1;

            public unsafe ref FFLDrawParam this[int index]
            {
                get
                {
                    fixed (FFLDrawParam* pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
