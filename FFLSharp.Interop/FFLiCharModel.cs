using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

public unsafe partial struct FFLiCharModel
{
    public FFLiCharInfo charInfo;

    public FFLCharModelDesc charModelDesc;

    public FFLExpression expression;

    public FFLiTextureTempObject* pTextureTempObject;

    [NativeTypeName("FFLDrawParam[12]")]
    public _drawParam_e__FixedBuffer drawParam;

    [NativeTypeName("void *[12]")]
    public _pShapeData_e__FixedBuffer pShapeData;

    public FFLiRenderTexture facelineRenderTexture;

    [NativeTypeName("FFLTexture *")]
    public void* pCapTexture;

    [NativeTypeName("FFLTexture *")]
    public void* pGlassTexture;

    [NativeTypeName("FFLTexture *")]
    public void* pNoselineTexture;

    public FFLiMaskTextures maskTextures;

    public FFLVec3 beardPos;

    public FFLVec3 hairPos;

    public FFLVec3 faceCenterPos;

    public FFLPartsTransform partsTransform;

    public FFLModelType modelType;

    [NativeTypeName("FFLBoundingBox[3]")]
    public _boundingBox_e__FixedBuffer boundingBox;

    public partial struct _drawParam_e__FixedBuffer
    {
        public FFLDrawParam e0;
        public FFLDrawParam e1;
        public FFLDrawParam e2;
        public FFLDrawParam e3;
        public FFLDrawParam e4;
        public FFLDrawParam e5;
        public FFLDrawParam e6;
        public FFLDrawParam e7;
        public FFLDrawParam e8;
        public FFLDrawParam e9;
        public FFLDrawParam e10;
        public FFLDrawParam e11;

        public ref FFLDrawParam this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLDrawParam> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 12);
    }

    public unsafe partial struct _pShapeData_e__FixedBuffer
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

    public partial struct _boundingBox_e__FixedBuffer
    {
        public FFLBoundingBox e0;
        public FFLBoundingBox e1;
        public FFLBoundingBox e2;

        public ref FFLBoundingBox this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLBoundingBox> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 3);
    }
}

public partial struct FFLiCharModel
{
}
