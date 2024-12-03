using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using FFLSharp.Interop;

namespace FFLSharp.VeldridBasicShaderSample
{
    /// <summary>
    /// Static class that wraps FFL methods to initialize, create a CharModel, and perform cleanup.
    /// </summary>
    static class FFLHelpers
    {
        /// <summary>
        /// Assumes FFLResHigh.dat is in the current working directory. You should probably change this.
        /// </summary>
        private const string FFLResourceHighFilename = "./FFLResHigh.dat";

        /// <summary>
        /// FFLStoreData representing JasmineChlora.
        /// </summary>
        public static readonly byte[] JasmineStoreData = new byte[96]
        {
            0x03, 0x00, 0x00, 0x40, 0xA0, 0x41, 0x38, 0xC4, 0xA0, 0x84, 0x00, 0x00, 0xDB, 0xB8, 0x87, 0x31,
            0xBE, 0x60, 0x2B, 0x2A, 0x2A, 0x42, 0x00, 0x00, 0x59, 0x2D, 0x4A, 0x00, 0x61, 0x00, 0x73, 0x00,
            0x6D, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x37,
            0x12, 0x10, 0x7B, 0x01, 0x21, 0x6E, 0x43, 0x1C, 0x0D, 0x64, 0xC7, 0x18, 0x00, 0x08, 0x1E, 0x82,
            0x0D, 0x00, 0x30, 0x41, 0xB3, 0x5B, 0x82, 0x6D, 0x00, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x69, 0x00,
            0x67, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x3A
        };
        /// <summary>
        /// FFLStoreData representing Bro. I know, insanely helpful.
        /// </summary>
        public static readonly byte[] BroStoreData = new byte[96]
        {
            0x03, 0x00, 0x00, 0x40, 0xdf, 0x6e, 0x67, 0x47, 0xaa, 0xc6, 0x47, 0x34, 0xdb, 0x6b, 0xfb, 0x75,
            0xbc, 0xbc, 0xb0, 0x1b, 0xf8, 0xa2, 0x00, 0x00, 0x00, 0x04, 0x62, 0x00, 0x72, 0x00, 0x6f, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x40,
            0x2c, 0x92, 0x39, 0x02, 0x02, 0x89, 0x44, 0x16, 0x66, 0x34, 0x46, 0x10, 0xcd, 0x12, 0x0d, 0x48,
            0x4f, 0x00, 0xe2, 0x28, 0x22, 0x42, 0x89, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2a, 0x4f
        };

        public static readonly uint DefaultTextureResolution = 512;

        /// <summary>
        /// Calls FFLInitCharModelCPUStep on an existing CharModel instance from data.
        /// </summary>
        /// <param name="pCharModel">FFLCharModel instance to be created before calling this.</param>
        /// <param name="pStoreDataBuffer">FFLStoreData representing the CharModel's data.</param>
        /// <returns>FFLResult representing if the CharModel was able to initialize properly.</returns>
        public unsafe static FFLResult CreateCharModelFromStoreData(ref FFLCharModel charModel, byte[] pStoreDataBuffer, FFLTextureCallback* callback)
        {
            FFLCharModelSource modelSource;
            fixed (byte* pBuffer = pStoreDataBuffer)
            {
                // Create the FFLCharModelSource struct
                modelSource = new FFLCharModelSource
                {
                    dataSource = FFLDataSource.FFL_DATA_SOURCE_STORE_DATA,
                    pBuffer = pBuffer,
                    index = 0
                };
            }

            const int expressionFlag = (1 << (int)FFLExpression.FFL_EXPRESSION_NORMAL) |
                                       (1 << (int)FFLExpression.FFL_EXPRESSION_BLINK);

            var modelDesc = new FFLCharModelDesc
            {
                resolution = (FFLResolution)DefaultTextureResolution,
                expressionFlag = expressionFlag,
                modelFlag = (uint)FFLModelFlag.FFL_MODEL_FLAG_NORMAL,
                resourceType = FFLResourceType.FFL_RESOURCE_TYPE_HIGH
            };

            Console.WriteLine("Calling FFLInitCharModelCPUStep");
            FFLResult result = FFL.InitCharModelCPUStepWithCallback(
                (FFLCharModel*)Unsafe.AsPointer(ref charModel),
                &modelSource,
                &modelDesc,
                callback//(FFLTextureCallback*)Unsafe.AsPointer(ref callback)
            );

            if (result != FFLResult.FFL_RESULT_OK)
            {
                throw new Exception($"FFLInitCharModelCPUStep failed with result: {result}");
                //return result;
            }

            return result;
        }


        /// <summary>
        /// Last result of FFLIsAvailable.
        /// </summary>
        public static bool IsAvailable = false;

        private static FFLResourceDesc _resourceDesc = new();

        /// <summary>
        /// Calls FFLInitRes() to initialize FFL and its resource. This is needed
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Indicates that the resource file does not exist.</exception>
        /// <exception cref="ArgumentException">Indicates that the file's size is not valid.</exception>
        /// <exception cref="Exception">Indicates FFL returned some error during initialization.</exception>
        public static FFLResult InitializeFFL()
        {
            Console.WriteLine("Before FFL initialization");

            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_HIGH, IntPtr.Zero, 0);
            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_MIDDLE, IntPtr.Zero, 0);

            byte[] fileData = File.ReadAllBytes(FFLResourceHighFilename); // Will throw exception if it does not exist
            int fileSize = fileData.Length;

            if (fileSize <= 0)
            {
                throw new ArgumentException($"Invalid file size for {FFLResourceHighFilename}");
            }

            IntPtr fileDataPtr = Marshal.AllocHGlobal(fileSize);
            Marshal.Copy(fileData, 0, fileDataPtr, fileSize);

            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_HIGH, fileDataPtr, (uint)fileSize);

            Console.WriteLine("Calling FFLInitResEx");

            FFLResult result;
            unsafe
            {
                fixed (FFLResourceDesc* pResourceDesc = &_resourceDesc)
                {
                    result = FFL.InitRes(FFLFontRegion.FFL_FONT_REGION_JP_US_EU, pResourceDesc);
                }
            }

            if (result != FFLResult.FFL_RESULT_OK)
            {
                Marshal.FreeHGlobal(fileDataPtr);
                throw new Exception($"FFLInitResEx() failed with result: {result}");
                //return result;
            }

            if (FFL.IsAvailable() == 0)
            {
                IsAvailable = false;
                Marshal.FreeHGlobal(fileDataPtr);
                throw new Exception("FFL is not available after initialization.");
                //return FFLResult.FFL_RESULT_ERROR;
            }

            FFL.InitResGPUStep(); // CanInitCharModel will fail if you don't do this
            Console.WriteLine("Exiting InitializeFFL()");
            return result;
        }
        private unsafe static void SetResourceData(FFLResourceType resourceType, IntPtr dataPtr, uint size)
        {
            int index = (int)resourceType;

            // Set the size field
            _resourceDesc.size[index] = size;

            // Set the pData pointer directly
            if (index == 0)
            {
                _resourceDesc.pData.e0 = (void*)dataPtr;
            }
            else if (index == 1)
            {
                _resourceDesc.pData.e1 = (void*)dataPtr;
            }
        }
        /// <summary>
        /// Calls FFLDeleteCharModel().
        /// </summary>
        /// <param name="charModel">CharModel reference.</param>
        public unsafe static void DeleteCharModel(ref FFLCharModel charModel)
        {
            FFL.DeleteCharModel((FFLCharModel*)Unsafe.AsPointer(ref charModel));
        }
        /// <summary>
        /// Frees resources and calls FFLExit().
        /// </summary>
        public unsafe static void CleanupFFL()
        {
            // Free the memory allocated for pData in gResourceDesc
            if (_resourceDesc.pData[0] != null)
            {
                Marshal.FreeHGlobal((IntPtr)_resourceDesc.pData[0]);
                _resourceDesc.pData[0] = null;
            }

            if (_resourceDesc.pData[1] != null)
            {
                Marshal.FreeHGlobal((IntPtr)_resourceDesc.pData[1]);
                _resourceDesc.pData[1] = null;
            }

            // Call FFLExit to clean up any internal FFL resources
            FFL.Exit();
        }
    }
}
