namespace FFLSharp.Interop
{
    public partial struct FFLiMaskTextures
    {
        [NativeTypeName("FFLTexture **[70]")]
        public _pRenderTextures_e__FixedBuffer pRenderTextures;

        public unsafe partial struct _pRenderTextures_e__FixedBuffer
        {
            public void** e0;
            public void** e1;
            public void** e2;
            public void** e3;
            public void** e4;
            public void** e5;
            public void** e6;
            public void** e7;
            public void** e8;
            public void** e9;
            public void** e10;
            public void** e11;
            public void** e12;
            public void** e13;
            public void** e14;
            public void** e15;
            public void** e16;
            public void** e17;
            public void** e18;
            public void** e19;
            public void** e20;
            public void** e21;
            public void** e22;
            public void** e23;
            public void** e24;
            public void** e25;
            public void** e26;
            public void** e27;
            public void** e28;
            public void** e29;
            public void** e30;
            public void** e31;
            public void** e32;
            public void** e33;
            public void** e34;
            public void** e35;
            public void** e36;
            public void** e37;
            public void** e38;
            public void** e39;
            public void** e40;
            public void** e41;
            public void** e42;
            public void** e43;
            public void** e44;
            public void** e45;
            public void** e46;
            public void** e47;
            public void** e48;
            public void** e49;
            public void** e50;
            public void** e51;
            public void** e52;
            public void** e53;
            public void** e54;
            public void** e55;
            public void** e56;
            public void** e57;
            public void** e58;
            public void** e59;
            public void** e60;
            public void** e61;
            public void** e62;
            public void** e63;
            public void** e64;
            public void** e65;
            public void** e66;
            public void** e67;
            public void** e68;
            public void** e69;

            public ref void** this[int index]
            {
                get
                {
                    fixed (void*** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }
}
