namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLDataSource : uint
    {
        FFL_DATA_SOURCE_OFFICIAL = 0,
        FFL_DATA_SOURCE_DEFAULT = 1,
        FFL_DATA_SOURCE_MIDDLE_DB = 2,
        FFL_DATA_SOURCE_STORE_DATA_OFFICIAL = 3,
        FFL_DATA_SOURCE_STORE_DATA = 4,
        FFL_DATA_SOURCE_BUFFER = 5,
        FFL_DATA_SOURCE_DIRECT_POINTER = 6,
    }
}
