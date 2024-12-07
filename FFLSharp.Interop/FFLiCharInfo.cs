namespace FFLSharp.Interop
{
    public unsafe partial struct FFLiCharInfo
    {
        [NativeTypeName("u32")]
        public uint miiVersion;

        [NativeTypeName("__AnonymousRecord_FFLiCharInfo_L19_C5")]
        public _parts_e__Struct parts;

        [NativeTypeName("u32")]
        public uint height;

        [NativeTypeName("u32")]
        public uint build;

        [NativeTypeName("u16[11]")]
        public fixed ushort name[11];

        [NativeTypeName("u16[11]")]
        public fixed ushort creatorName[11];

        public FFLGender gender;

        [NativeTypeName("u32")]
        public uint birthMonth;

        [NativeTypeName("u32")]
        public uint birthDay;

        public FFLFavoriteColor favoriteColor;

        [NativeTypeName("u8")]
        public byte favoriteMii;

        [NativeTypeName("u8")]
        public byte copyable;

        [NativeTypeName("u8")]
        public byte ngWord;

        [NativeTypeName("u8")]
        public byte localOnly;

        [NativeTypeName("u32")]
        public uint regionMove;

        public FFLFontRegion fontRegion;

        [NativeTypeName("u32")]
        public uint pageIndex;

        [NativeTypeName("u32")]
        public uint slotIndex;

        public FFLBirthPlatform birthPlatform;

        public FFLCreateID creatorID;

        [NativeTypeName("u16")]
        public ushort _112;

        [NativeTypeName("u32")]
        public uint authorType;

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
