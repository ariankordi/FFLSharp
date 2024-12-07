using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLCharModelDesc
    {
        public FFLResolution resolution;

        [NativeTypeName("__AnonymousRecord_FFLCharModelDesc_L17_C5")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("u32")]
        public uint modelFlag;

        public FFLResourceType resourceType;

        public ref uint expressionFlag
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->expressionFlag;
                }
            }
        }

        public ref FFLAllExpressionFlag allExpressionFlag
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->allExpressionFlag;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("u32")]
            public uint expressionFlag;

            [FieldOffset(0)]
            public FFLAllExpressionFlag allExpressionFlag;
        }
    }
}
