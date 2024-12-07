namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiFacelineTextureTempObject
    {
        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceLine;

        public FFLDrawParam drawParamFaceLine;

        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceMake;

        public FFLDrawParam drawParamFaceMake;

        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceBeard;

        public FFLDrawParam drawParamFaceBeard;

        public void* _144;

        public void* _148;
    }
}
