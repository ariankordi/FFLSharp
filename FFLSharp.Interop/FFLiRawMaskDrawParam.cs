using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

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

        public ref FFLDrawParam this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLDrawParam> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 2);
    }

    public partial struct _drawParamRawMaskPartsEyebrow_e__FixedBuffer
    {
        public FFLDrawParam e0;
        public FFLDrawParam e1;

        public ref FFLDrawParam this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLDrawParam> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 2);
    }

    public partial struct _drawParamRawMaskPartsMustache_e__FixedBuffer
    {
        public FFLDrawParam e0;
        public FFLDrawParam e1;

        public ref FFLDrawParam this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLDrawParam> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 2);
    }
}
