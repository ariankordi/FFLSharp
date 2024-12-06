using System.Diagnostics;
using System.Runtime.InteropServices;
using FFLSharp.Interop;

namespace FFLSharp
{
    /// <summary>
    /// Class to simplify input parameters for initializing a CharModel.
    /// Allocates the input byte buffer into unmanaged memory and frees it after disposal.
    /// </summary>
    public class CharModelInitParam : IDisposable
    {
        public readonly FFLCharModelDesc ModelDesc;
        public readonly FFLCharModelSource ModelSource;

        private IntPtr _pBuffer = IntPtr.Zero;

        /// <summary>
        /// Creates a CharModelInitParam for use to initialize a CharModel in other functions.
        /// The buffer you specify will be allocated into unmanaged memory and then freed.
        /// </summary>
        /// <param name="resolution">Resolution of mask and faceline textures.</param>
        /// <param name="dataSource">FFLDataSource enum value, default is FFL_DATA_SOURCE_STORE_DATA.</param>
        /// <param name="data">Byte array to be allocated into unmanaged memory.</param>
        /// <param name="index">Index of a database or MiddleDB.</param>
        /// <param name="expressionFlag">Expression flag which can be created with the static method MakeExpressionFlag.</param>
        /// <param name="modelFlag">FFLModelFlag enum value, for hats and helmets.</param>
        /// <param name="resourceType">Resource type, default is in FFLManager.ResourceType.</param>
        public CharModelInitParam(
            FFLResolution? resolution = null,
            FFLDataSource? dataSource = null,
            byte[]? data = null,
            ushort? index = null,
            uint? expressionFlag = null,
            FFLModelFlag? modelFlag = null,
            FFLResourceType? resourceType = null)
        {
            // Create FFLCharModelDesc instance with: resolution, modelFlag, resourceType.
            ModelDesc = new FFLCharModelDesc()
            {
                resolution = resolution ?? FFLManager.TextureResolution, // Default is static in FFLManager.
                modelFlag = (uint)(modelFlag ?? FFLModelFlag.FFL_MODEL_FLAG_NORMAL),
                resourceType = resourceType ?? FFLManager.ResourceType, // Default is static in FFLManager.
                // Set expression flag or default as normal.
                // This can be created with the static method MakeExpressionFlag
                expressionFlag = expressionFlag ?? (1 << (int)FFLExpression.FFL_EXPRESSION_NORMAL)
            };

            unsafe
            {
                if (data != null)
                {
                    // Allocate unmanaged memory for the buffer.
                    _pBuffer = Marshal.AllocHGlobal(data.Length);
                    // Copy data into the unmanaged buffer.
                    Marshal.Copy(data, 0, _pBuffer, data.Length);
                }

                // Create FFLCharModelSource with: dataSource, index, and unmanaged buffer pointer.
                ModelSource = new FFLCharModelSource
                {
                    // Assume FFLStoreData is being passed in by default.
                    dataSource = dataSource ?? FFLDataSource.FFL_DATA_SOURCE_STORE_DATA,
                    index = index ?? (ushort)0, // NOTE: Not actually used right now.
                    pBuffer = (void*)_pBuffer // Will be initialized to zero.
                };
            }

            Debug.Assert((index ?? 0) == 0); // TODO: REMOVE THIS when index is usable
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_pBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_pBuffer);
                _pBuffer = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Static method to create an expressionFlag based on multiple expressions.
        /// An expression flag is a set of bits, where for each bit that is set,
        /// it indicates that that expression's mask should be rendered.
        /// </summary>
        /// <param name="expressions">Multiple FFLExpression enum values to combine into one expression flag.</param>
        /// <returns>An expression flag usable for initializing a CharModel with.</returns>
        public static uint MakeExpressionFlag(params FFLExpression[] expressions)
        {
            uint flag = 0;

            foreach (var expression in expressions)
            {
                // Shift 1 by the enum's int value and binary OR it with the result.
                flag |= 1U << (int)expression;
            }

            return flag;
        }

    }
}
