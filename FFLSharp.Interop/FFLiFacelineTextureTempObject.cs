namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiFacelineTextureTempObject
    {
        public FFLDrawParam drawParamFaceMake;

        public FFLDrawParam drawParamFaceLine;

        public FFLDrawParam drawParamFaceBeard;

        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceLine;

        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceMake;

        [NativeTypeName("FFLTexture *")]
        public void* pTextureFaceBeard;

        public void* _144;

        public void* _148;
    }
}
