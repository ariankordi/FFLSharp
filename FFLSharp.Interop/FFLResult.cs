namespace FFLSharp.Interop
{
    [NativeTypeName("unsigned int")]
    public enum FFLResult : uint
    {
        FFL_RESULT_OK = 0,
        FFL_RESULT_ERROR = 1,
        FFL_RESULT_HDB_EMPTY = 2,
        FFL_RESULT_FILE_INVALID = 3,
        FFL_RESULT_MANAGER_NOT_CONSTRUCT = 4,
        FFL_RESULT_FILE_LOAD_ERROR = 5,
        FFL_RESULT_FILE_SAVE_ERROR = 7,
        FFL_RESULT_RES_FS_ERROR = 9,
        FFL_RESULT_ODB_EMPTY = 10,
        FFL_RESULT_OUT_OF_MEMORY = 12,
        FFL_RESULT_UNKNOWN_17 = 17,
        FFL_RESULT_FS_ERROR = 18,
        FFL_RESULT_FS_NOT_FOUND = 19,
    }
}
