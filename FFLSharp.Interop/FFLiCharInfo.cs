namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiCharInfo
    {
        [NativeTypeName("s32")]
        public int miiVersion;

        [NativeTypeName("__AnonymousRecord_FFLiCharInfo_L116_C5")]
        public _parts_e__Struct parts;

        [NativeTypeName("s32")]
        public int height;

        [NativeTypeName("s32")]
        public int build;

        [NativeTypeName("u16[11]")]
        public fixed ushort name[11];

        [NativeTypeName("u16[11]")]
        public fixed ushort creatorName[11];

        [NativeTypeName("s32")]
        public int gender;

        [NativeTypeName("s32")]
        public int birthMonth;

        [NativeTypeName("s32")]
        public int birthDay;

        [NativeTypeName("s32")]
        public int favoriteColor;

        [NativeTypeName("u8")]
        public byte favoriteMii;

        [NativeTypeName("u8")]
        public byte copyable;

        [NativeTypeName("u8")]
        public byte ngWord;

        [NativeTypeName("u8")]
        public byte localOnly;

        [NativeTypeName("s32")]
        public int regionMove;

        [NativeTypeName("s32")]
        public int fontRegion;

        [NativeTypeName("s32")]
        public int pageIndex;

        [NativeTypeName("s32")]
        public int slotIndex;

        [NativeTypeName("s32")]
        public int birthPlatform;

        public FFLCreateID createID;

        [NativeTypeName("u16")]
        public ushort padding_0;

        [NativeTypeName("s32")]
        public int authorType;

        public FFLiAuthorID authorID;

        public partial struct _parts_e__Struct
        {
            [NativeTypeName("s32")]
            public int faceType;

            [NativeTypeName("s32")]
            public int facelineColor;

            [NativeTypeName("s32")]
            public int faceLine;

            [NativeTypeName("s32")]
            public int faceMakeup;

            [NativeTypeName("s32")]
            public int hairType;

            [NativeTypeName("s32")]
            public int hairColor;

            [NativeTypeName("s32")]
            public int hairDir;

            [NativeTypeName("s32")]
            public int eyeType;

            [NativeTypeName("s32")]
            public int eyeColor;

            [NativeTypeName("s32")]
            public int eyeScale;

            [NativeTypeName("s32")]
            public int eyeScaleY;

            [NativeTypeName("s32")]
            public int eyeRotate;

            [NativeTypeName("s32")]
            public int eyeSpacingX;

            [NativeTypeName("s32")]
            public int eyePositionY;

            [NativeTypeName("s32")]
            public int eyebrowType;

            [NativeTypeName("s32")]
            public int eyebrowColor;

            [NativeTypeName("s32")]
            public int eyebrowScale;

            [NativeTypeName("s32")]
            public int eyebrowScaleY;

            [NativeTypeName("s32")]
            public int eyebrowRotate;

            [NativeTypeName("s32")]
            public int eyebrowSpacingX;

            [NativeTypeName("s32")]
            public int eyebrowPositionY;

            [NativeTypeName("s32")]
            public int noseType;

            [NativeTypeName("s32")]
            public int noseScale;

            [NativeTypeName("s32")]
            public int nosePositionY;

            [NativeTypeName("s32")]
            public int mouthType;

            [NativeTypeName("s32")]
            public int mouthColor;

            [NativeTypeName("s32")]
            public int mouthScale;

            [NativeTypeName("s32")]
            public int mouthScaleY;

            [NativeTypeName("s32")]
            public int mouthPositionY;

            [NativeTypeName("s32")]
            public int mustacheType;

            [NativeTypeName("s32")]
            public int beardType;

            [NativeTypeName("s32")]
            public int beardColor;

            [NativeTypeName("s32")]
            public int mustacheScale;

            [NativeTypeName("s32")]
            public int mustachePositionY;

            [NativeTypeName("s32")]
            public int glassType;

            [NativeTypeName("s32")]
            public int glassColor;

            [NativeTypeName("s32")]
            public int glassScale;

            [NativeTypeName("s32")]
            public int glassPositionY;

            [NativeTypeName("s32")]
            public int moleType;

            [NativeTypeName("s32")]
            public int moleScale;

            [NativeTypeName("s32")]
            public int molePositionX;

            [NativeTypeName("s32")]
            public int molePositionY;
        }
    }
}
