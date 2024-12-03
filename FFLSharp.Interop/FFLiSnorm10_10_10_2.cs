namespace FFLSharp.Interop;

public partial struct FFLiSnorm10_10_10_2
{
    public uint _bitfield;

    [NativeTypeName("u32 : 10")]
    public uint x
    {
        readonly get
        {
            return _bitfield & 0x3FFu;
        }

        set
        {
            _bitfield = (_bitfield & ~0x3FFu) | (value & 0x3FFu);
        }
    }

    [NativeTypeName("u32 : 10")]
    public uint y
    {
        readonly get
        {
            return (_bitfield >> 10) & 0x3FFu;
        }

        set
        {
            _bitfield = (_bitfield & ~(0x3FFu << 10)) | ((value & 0x3FFu) << 10);
        }
    }

    [NativeTypeName("u32 : 10")]
    public uint z
    {
        readonly get
        {
            return (_bitfield >> 20) & 0x3FFu;
        }

        set
        {
            _bitfield = (_bitfield & ~(0x3FFu << 20)) | ((value & 0x3FFu) << 20);
        }
    }

    [NativeTypeName("u32 : 2")]
    public uint w
    {
        readonly get
        {
            return (_bitfield >> 30) & 0x3u;
        }

        set
        {
            _bitfield = (_bitfield & ~(0x3u << 30)) | ((value & 0x3u) << 30);
        }
    }
}
