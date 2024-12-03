using System;
using System.Runtime.InteropServices;

namespace FFLSharp.Interop;

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

        public ref FFLAttributeBuffer this[int index]
        {
            get
            {
                return ref AsSpan()[index];
            }
        }

        public Span<FFLAttributeBuffer> AsSpan() => MemoryMarshal.CreateSpan(ref e0, 5);
    }
}
