using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public unsafe partial struct FFLAdditionalInfo
    {
        [NativeTypeName("u16[11]")]
        public fixed ushort name[11];

        [NativeTypeName("u16[11]")]
        public fixed ushort creator[11];

        public FFLCreateID createID;

        public FFLColor skinColor;

        [NativeTypeName("__AnonymousRecord_FFLAdditionalInfo_L17_C5")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("u8")]
        public byte facelineType;

        [NativeTypeName("u8")]
        public byte hairType;

        public uint hairFlip
        {
            get
            {
                return Anonymous.Anonymous.hairFlip;
            }

            set
            {
                Anonymous.Anonymous.hairFlip = value;
            }
        }

        public uint fontRegion
        {
            get
            {
                return Anonymous.Anonymous.fontRegion;
            }

            set
            {
                Anonymous.Anonymous.fontRegion = value;
            }
        }

        public uint ngWord
        {
            get
            {
                return Anonymous.Anonymous.ngWord;
            }

            set
            {
                Anonymous.Anonymous.ngWord = value;
            }
        }

        public uint build
        {
            get
            {
                return Anonymous.Anonymous.build;
            }

            set
            {
                Anonymous.Anonymous.build = value;
            }
        }

        public uint height
        {
            get
            {
                return Anonymous.Anonymous.height;
            }

            set
            {
                Anonymous.Anonymous.height = value;
            }
        }

        public uint favoriteColor
        {
            get
            {
                return Anonymous.Anonymous.favoriteColor;
            }

            set
            {
                Anonymous.Anonymous.favoriteColor = value;
            }
        }

        public uint birthDay
        {
            get
            {
                return Anonymous.Anonymous.birthDay;
            }

            set
            {
                Anonymous.Anonymous.birthDay = value;
            }
        }

        public uint birthMonth
        {
            get
            {
                return Anonymous.Anonymous.birthMonth;
            }

            set
            {
                Anonymous.Anonymous.birthMonth = value;
            }
        }

        public uint gender
        {
            get
            {
                return Anonymous.Anonymous.gender;
            }

            set
            {
                Anonymous.Anonymous.gender = value;
            }
        }

        public ref uint flags
        {
            get
            {
                fixed (_Anonymous_e__Union* pField = &Anonymous)
                {
                    return ref pField->flags;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_FFLAdditionalInfo_L19_C9")]
            public _Anonymous_1_e__Struct Anonymous;

            [FieldOffset(0)]
            [NativeTypeName("u32")]
            public uint flags;

            public partial struct _Anonymous_1_e__Struct
            {
                public uint _bitfield;

                [NativeTypeName("u32 : 1")]
                public uint hairFlip
                {
                    get
                    {
                        return _bitfield & 0x1u;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
                    }
                }

                [NativeTypeName("u32 : 2")]
                public uint fontRegion
                {
                    get
                    {
                        return (_bitfield >> 1) & 0x3u;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x3u << 1)) | ((value & 0x3u) << 1);
                    }
                }

                [NativeTypeName("u32 : 1")]
                public uint ngWord
                {
                    get
                    {
                        return (_bitfield >> 3) & 0x1u;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3);
                    }
                }

                [NativeTypeName("u32 : 7")]
                public uint build
                {
                    get
                    {
                        return (_bitfield >> 4) & 0x7Fu;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x7Fu << 4)) | ((value & 0x7Fu) << 4);
                    }
                }

                [NativeTypeName("u32 : 7")]
                public uint height
                {
                    get
                    {
                        return (_bitfield >> 11) & 0x7Fu;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x7Fu << 11)) | ((value & 0x7Fu) << 11);
                    }
                }

                [NativeTypeName("u32 : 4")]
                public uint favoriteColor
                {
                    get
                    {
                        return (_bitfield >> 18) & 0xFu;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0xFu << 18)) | ((value & 0xFu) << 18);
                    }
                }

                [NativeTypeName("u32 : 5")]
                public uint birthDay
                {
                    get
                    {
                        return (_bitfield >> 22) & 0x1Fu;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1Fu << 22)) | ((value & 0x1Fu) << 22);
                    }
                }

                [NativeTypeName("u32 : 4")]
                public uint birthMonth
                {
                    get
                    {
                        return (_bitfield >> 27) & 0xFu;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0xFu << 27)) | ((value & 0xFu) << 27);
                    }
                }

                [NativeTypeName("u32 : 1")]
                public uint gender
                {
                    get
                    {
                        return (_bitfield >> 31) & 0x1u;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1u << 31)) | ((value & 0x1u) << 31);
                    }
                }
            }
        }
    }
}
