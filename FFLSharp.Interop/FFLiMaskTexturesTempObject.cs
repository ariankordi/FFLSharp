namespace FFLSharp.Interop;

public unsafe partial struct FFLiMaskTexturesTempObject
{
    public FFLiPartsTextures partsTextures;

    [NativeTypeName("FFLiRawMaskDrawParam *[70]")]
    public _pRawMaskDrawParam_e__FixedBuffer pRawMaskDrawParam;

    public void* _84;

    [NativeTypeName("void *[70]")]
    public __88_e__FixedBuffer _88;

    public unsafe partial struct _pRawMaskDrawParam_e__FixedBuffer
    {
        public FFLiRawMaskDrawParam* e0;
        public FFLiRawMaskDrawParam* e1;
        public FFLiRawMaskDrawParam* e2;
        public FFLiRawMaskDrawParam* e3;
        public FFLiRawMaskDrawParam* e4;
        public FFLiRawMaskDrawParam* e5;
        public FFLiRawMaskDrawParam* e6;
        public FFLiRawMaskDrawParam* e7;
        public FFLiRawMaskDrawParam* e8;
        public FFLiRawMaskDrawParam* e9;
        public FFLiRawMaskDrawParam* e10;
        public FFLiRawMaskDrawParam* e11;
        public FFLiRawMaskDrawParam* e12;
        public FFLiRawMaskDrawParam* e13;
        public FFLiRawMaskDrawParam* e14;
        public FFLiRawMaskDrawParam* e15;
        public FFLiRawMaskDrawParam* e16;
        public FFLiRawMaskDrawParam* e17;
        public FFLiRawMaskDrawParam* e18;
        public FFLiRawMaskDrawParam* e19;
        public FFLiRawMaskDrawParam* e20;
        public FFLiRawMaskDrawParam* e21;
        public FFLiRawMaskDrawParam* e22;
        public FFLiRawMaskDrawParam* e23;
        public FFLiRawMaskDrawParam* e24;
        public FFLiRawMaskDrawParam* e25;
        public FFLiRawMaskDrawParam* e26;
        public FFLiRawMaskDrawParam* e27;
        public FFLiRawMaskDrawParam* e28;
        public FFLiRawMaskDrawParam* e29;
        public FFLiRawMaskDrawParam* e30;
        public FFLiRawMaskDrawParam* e31;
        public FFLiRawMaskDrawParam* e32;
        public FFLiRawMaskDrawParam* e33;
        public FFLiRawMaskDrawParam* e34;
        public FFLiRawMaskDrawParam* e35;
        public FFLiRawMaskDrawParam* e36;
        public FFLiRawMaskDrawParam* e37;
        public FFLiRawMaskDrawParam* e38;
        public FFLiRawMaskDrawParam* e39;
        public FFLiRawMaskDrawParam* e40;
        public FFLiRawMaskDrawParam* e41;
        public FFLiRawMaskDrawParam* e42;
        public FFLiRawMaskDrawParam* e43;
        public FFLiRawMaskDrawParam* e44;
        public FFLiRawMaskDrawParam* e45;
        public FFLiRawMaskDrawParam* e46;
        public FFLiRawMaskDrawParam* e47;
        public FFLiRawMaskDrawParam* e48;
        public FFLiRawMaskDrawParam* e49;
        public FFLiRawMaskDrawParam* e50;
        public FFLiRawMaskDrawParam* e51;
        public FFLiRawMaskDrawParam* e52;
        public FFLiRawMaskDrawParam* e53;
        public FFLiRawMaskDrawParam* e54;
        public FFLiRawMaskDrawParam* e55;
        public FFLiRawMaskDrawParam* e56;
        public FFLiRawMaskDrawParam* e57;
        public FFLiRawMaskDrawParam* e58;
        public FFLiRawMaskDrawParam* e59;
        public FFLiRawMaskDrawParam* e60;
        public FFLiRawMaskDrawParam* e61;
        public FFLiRawMaskDrawParam* e62;
        public FFLiRawMaskDrawParam* e63;
        public FFLiRawMaskDrawParam* e64;
        public FFLiRawMaskDrawParam* e65;
        public FFLiRawMaskDrawParam* e66;
        public FFLiRawMaskDrawParam* e67;
        public FFLiRawMaskDrawParam* e68;
        public FFLiRawMaskDrawParam* e69;

        public ref FFLiRawMaskDrawParam* this[int index]
        {
            get
            {
                fixed (FFLiRawMaskDrawParam** pThis = &e0)
                {
                    return ref pThis[index];
                }
            }
        }
    }

    public unsafe partial struct __88_e__FixedBuffer
    {
        public void* e0;
        public void* e1;
        public void* e2;
        public void* e3;
        public void* e4;
        public void* e5;
        public void* e6;
        public void* e7;
        public void* e8;
        public void* e9;
        public void* e10;
        public void* e11;
        public void* e12;
        public void* e13;
        public void* e14;
        public void* e15;
        public void* e16;
        public void* e17;
        public void* e18;
        public void* e19;
        public void* e20;
        public void* e21;
        public void* e22;
        public void* e23;
        public void* e24;
        public void* e25;
        public void* e26;
        public void* e27;
        public void* e28;
        public void* e29;
        public void* e30;
        public void* e31;
        public void* e32;
        public void* e33;
        public void* e34;
        public void* e35;
        public void* e36;
        public void* e37;
        public void* e38;
        public void* e39;
        public void* e40;
        public void* e41;
        public void* e42;
        public void* e43;
        public void* e44;
        public void* e45;
        public void* e46;
        public void* e47;
        public void* e48;
        public void* e49;
        public void* e50;
        public void* e51;
        public void* e52;
        public void* e53;
        public void* e54;
        public void* e55;
        public void* e56;
        public void* e57;
        public void* e58;
        public void* e59;
        public void* e60;
        public void* e61;
        public void* e62;
        public void* e63;
        public void* e64;
        public void* e65;
        public void* e66;
        public void* e67;
        public void* e68;
        public void* e69;

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
