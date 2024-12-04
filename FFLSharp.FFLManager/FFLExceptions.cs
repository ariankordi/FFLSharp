using FFLSharp.Interop;

namespace FFLSharp
{
    /// <summary>
    /// Base exception type for all exceptions based on FFLResult.
    /// </summary>
    public class FFLResultException : Exception
    {
        public readonly FFLResult Result;

        public FFLResultException(FFLResult result, string message = null, Exception innerException = null)
            : base(message ?? $"From FFLResult: {result}", innerException)
        {
            Result = result;
        }

        /// <summary>
        /// Throws an exception if FFLResult reflects an error.
        /// </summary>
        /// <param name="result">Input FFLResult.</param>
        public static void HandleResult(FFLResult result)
        {
            switch (result)
            {
                case FFLResult.FFL_RESULT_ERROR:
                    throw new ResultWrongParam(result);
                case FFLResult.FFL_RESULT_FILE_INVALID:
                    throw new ResultBroken(result);
                case FFLResult.FFL_RESULT_MANAGER_NOT_CONSTRUCT:
                    throw new ResultNotAvailable(result);
                case FFLResult.FFL_RESULT_FILE_LOAD_ERROR:
                    throw new ResultFatal(result);
                case FFLResult.FFL_RESULT_OK:
                    return; // All is OK, return nothing.
                default:
                    throw new FFLResultException(result);
            }
        }
    }

    /// <summary>
    /// Exception reflecting FFL_RESULT_WRONG_PARAM / FFL_RESULT_ERROR.
    /// This is the most common error thrown in FFL. It usually
    /// means that input parameters are invalid.
    /// So many cases this is thrown: parts index is out of bounds,
    /// CharModelCreateParam is malformed, FFLDataSource is invalid, FFLInitResEx
    /// parameters are null or invalid... Many different causes, very much an annoying error.
    /// </summary>
    public class ResultWrongParam : FFLResultException
    {
        public ResultWrongParam(FFLResult result, string message
            = "FFL returned FFL_RESULT_WRONG_PARAM. This usually means parameters "
            + "going into a certain function were interpreted as invalid.")
            : base(result, message) { }
    }

    /// <summary>
    /// Exception reflecting FFL_RESULT_BROKEN / FFL_RESULT_FILE_INVALID.
    /// </summary>
    public class ResultBroken : FFLResultException
    {
        public ResultBroken(FFLResult result, string message
            = "FFL returned FFL_RESULT_BROKEN. This usually indicates invalid underlying data.")
            : base(result, message) { }
    }

    /// <summary>
    /// When this is thrown and you initialized the resource
    /// from cache (all of the time), it means the resource header failed verification.
    /// </summary>
    public class BrokenInitRes : ResultBroken
    {
        public BrokenInitRes(FFLResult result)
            : base(result, "The header for the FFL resource is probably invalid. Check the version and magic, should be \"FFRA\" or \"ARFF\".") { }
    }

    /// <summary>
    /// Thrown when: CRC16 fails, CharInfo verification fails, or failing
    /// to fetch from a database (rare?)
    /// </summary>
    public class BrokenInitModel : ResultBroken
    {
        public BrokenInitModel(FFLResult result)
            : base(result, "FFLInitCharModelCPUStep failed probably because your data failed CRC or CharInfo verification (FFLiVerifyCharInfoWithReason).") { }
    }


    /// <summary>
    /// Exception reflecting FFL_RESULT_NOT_AVAILABLE / FFL_RESULT_MANAGER_NOT_CONSTRUCT.
    /// This is seen when FFLiManager is not constructed, which it is not when FFLInitResEx fails
    /// or was never called to begin with.
    /// </summary>
    public class ResultNotAvailable : FFLResultException
    {
        public ResultNotAvailable(FFLResult result)
            : base(result, "Tried to call an FFL method when FFLManager is not constructed (FFL is not initialized properly).") { }
    }

    /// <summary>
    /// Exception reflecting FFL_RESULT_FATAL / FFL_RESULT_FILE_LOAD_ERROR.
    /// This error indicates database file load errors or failures from FFLiResourceLoader.
    /// </summary>
    public class ResultFatal : FFLResultException
    {
        public ResultFatal(FFLResult result)
            : base(result, "Failed to uncompress or load a specific asset from the FFL resource file.") { }
    }

    /* Errors that are not handled here and that I cannot bother handling here:
     * (TL;DR: Most are database, FS, or other Cafe platform related.)
     * 2. FFL_RESULT_HDB_EMPTY/FFL_RESULT_DB_NO_DATA      - Has to do with the hidden DB, do not care.
     * 6. FFL_RESULT_LOAD_FAIL/FFL_RESULT_UNKNOWN_6       - I/O error for database stuff.
     * 7. FFL_RESULT_SAVE_FAIL/FFL_RESULT_FILE_SAVE_ERROR - I/O error for database stuff.
     * 9. FFL_RESULT_NAND_COMMAND_FAIL/FFL_RESULT_RES_FS_ERROR - I/O error when reading database from FS directly which we are not.
     * 10. FFL_RESULT_FORMATTED/FFL_RESULT_ODB_EMPTY      - When the FFL_ODB database is empty.
     * 12. FFL_RESULT_TEMP_MEMORY_SHORT/FFL_RESULT_OUT_OF_MEMORY - Only seen for database operations.
     * 17. FFL_RESULT_FAILED_ACT/FFL_RESULT_UNKNOWN_17    - nn::act::Finalize().IsFailure() or nn::act::GetTransferableIdEx(...) failed.
                                                          - Not even applicable here. Also TIL that author ID is derived from this ^^
     * 18. FFL_RESULT_FAILED_FS/FFL_RESULT_FS_ERROR       - Generic returned by FFLiConvertFSStatusToFFLResult, we are not even using that stuff.
     * 19. FFL_RESULT_RUN_MII_STUDIO/FFL_RESULT_FS_NOT_FOUND - Returned when any file is not found in FFLiDatabaseFileAccessor. Not applicable.
     * Errors that are not here are not used in the decomp and honestly most are still not applicable. Here they are:
     * 6 (FFL_RESULT_LOAD_FAIL), 8 (FFL_RESULT_BUSY), 11 (FFL_RESULT_DB_FULL), 13 (FFL_RESULT_INTERNAL0),
     * 14 (FFL_RESULT_INTERNAL1), 15 (FFL_RESULT_INTERNAL2), 16 (FFL_RESULT_FAILED_SYSTEM0)
     */

    /// <summary>
    /// Exception thrown when the mask is set to an expression that
    /// the CharModel was never initialized to, which can't happen
    /// because that mask texture does not exist on the CharModel.
    /// </summary>
    public class ExpressionNotSet : ArgumentException
    {
        public readonly FFLExpression Expression;
        public ExpressionNotSet(FFLExpression expression)
            : base($"Attempted to set expression {expression}, but the CharModel was "
                  + "not initialized with that expression. You must add this to the "
                  + "expression flag and re-initialize the CharModel.")
        {
            Expression = expression;
        }
    }
}
