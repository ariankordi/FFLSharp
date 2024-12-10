using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFLSharp.Interop;

namespace FFLSharp
{
    /// <summary>
    /// Static class that wraps FFL methods to read, initialize, and cleanup the resource.
    /// Also provides helper methods to create and delete CharModels.
    /// NOTE: Includes Dispose() that frees resources and calls FFLExit.
    /// </summary>
    public static class FFLManager
    {
        /// <summary>
        /// Assumes FFLResHigh.dat is in the current working directory. You should probably change this.
        /// </summary>
        private const string _resourceHighFilename = "./FFLResHigh.dat";


        /// <summary>
        /// Preferred resource type - highest that was loaded. Currently this is constant.
        /// </summary>
        public static FFLResourceType ResourceType { get; private set; } = FFLResourceType.FFL_RESOURCE_TYPE_HIGH;
        /// <summary>
        /// Default texture resolution.
        /// </summary>
        public static readonly FFLResolution TextureResolution = (FFLResolution)512;

        /// <summary>
        /// Represents if FFL has been initialized successfully.
        /// </summary>
        public static bool IsAvailable { get; private set; } = false;

        /// <summary>
        /// Structure where the size and data pointer to resources are stored.
        /// FFL accesses this on every FFLInitCharModelCPUStep call.
        /// </summary>
        private static FFLResourceDesc _resourceDesc = new FFLResourceDesc();

        private const uint _resourceHeaderDefaultSize = 18944; // sizeof(FFLiResourceHeaderDefaultData)

        /// <summary>
        /// Calls FFLInitCharModelCPUStep on an existing CharModel instance, initializing it.
        /// </summary>
        /// <param name="charModel">FFLCharModel instance to be created before calling this.</param>
        /// <param name="pStoreDataBuffer">FFLStoreData representing the CharModel's data.</param>
        /// <param name="pCallback">Pointer to your FFLTextureCallback instance. If this is null, you need to use FFLSetTextureCallback to set a global callback.</param>
        public static unsafe void InitCharModelFromStoreData(ref FFLCharModel charModel, byte[] pStoreDataBuffer, FFLTextureCallback* pCallback = null)
        {
            FFLCharModelSource modelSource;
            fixed (byte* pBuffer = pStoreDataBuffer)
            {
                // Assume input is always StoreData.
                modelSource = new FFLCharModelSource
                {
                    dataSource = FFLDataSource.FFL_DATA_SOURCE_STORE_DATA,
                    pBuffer = pBuffer,
                    index = 0
                };
            }

            // TODO: REPLACE THIS
            const uint expressionFlag = (1 << (int)FFLExpression.FFL_EXPRESSION_NORMAL) |
                                        (1 << (int)FFLExpression.FFL_EXPRESSION_BLINK);

            var modelDesc = new FFLCharModelDesc
            {
                resolution = TextureResolution,
                expressionFlag = expressionFlag,
                modelFlag = (uint)FFLModelFlag.FFL_MODEL_FLAG_NORMAL,
                resourceType = ResourceType
            };

            Console.WriteLine("Calling FFLInitCharModelCPUStep");

            // This call is prone to failure if the resource cannot be
            // loaded, or textures cannot be loaded, data cannot be
            // verified or allocations fail and segfault somehow.

            FFLResult result;
            fixed (FFLCharModel* pCharModel = &charModel)
            {
                result = FFL.InitCharModelCPUStepWithCallback(
                    pCharModel,
                    &modelSource,
                    &modelDesc,
                    pCallback//(FFLTextureCallback*)Unsafe.AsPointer(ref callback)
                );
            }

            // Throw a more specific exception for this result.
            if (result == FFLResult.FFL_RESULT_FILE_INVALID)
                throw new BrokenInitModel(result);

            // Handle all other results.
            FFLResultException.HandleResult(result);

            // FFLDeleteCharModel needs to be called when finished.
        }

        /// <summary>
        /// Overload to initialize using an ITextureManager instead of an FFLTextureCallback pointer.
        /// </summary>
        public static unsafe void InitCharModelFromStoreData(ref FFLCharModel charModel, byte[] pStoreDataBuffer, ITextureManager textureManager)
        {
            FFLTextureCallback* pCallback;
            pCallback = textureManager.GetTextureCallback();

            InitCharModelFromStoreData(ref charModel, pStoreDataBuffer, pCallback);
        }

        /// <summary>
        /// Creates and initializes an FFLCharModel instance using a CharModelInitParam.
        /// </summary>
        /// <param name="param">Instance of CharModelInitParam containing configuration.</param>
        public static unsafe FFLCharModel CreateCharModel(CharModelInitParam param, ITextureManager textureManager)
        {
            FFLTextureCallback* pCallback;
            // Get the texture callback to pass into FFLInitCharModelCPUStepWithCallback.
            pCallback = textureManager.GetTextureCallback();

            if (param == null)
                throw new ArgumentNullException(nameof(param));

            // Create an FFLCharModel instance.
            FFLCharModel charModel = new FFLCharModel();

            // Pin the managed structures.
            fixed (FFLCharModelSource* pModelSource = &param.ModelSource)
            fixed (FFLCharModelDesc* pModelDesc = &param.ModelDesc)
            {
                Console.WriteLine("Calling FFLInitCharModelCPUStepWithCallback");

                FFLResult result = FFL.InitCharModelCPUStepWithCallback(
                    &charModel,
                    pModelSource,
                    pModelDesc,
                    pCallback
                );

                // Free the unmanaged buffer immediately because it
                // is not needed after FFLInitCharModelCPUStep.
                param.Dispose();

                // Handle specific result
                if (result == FFLResult.FFL_RESULT_FILE_INVALID)
                    throw new BrokenInitModel(result);

                // Handle all other results
                FFLResultException.HandleResult(result);
            }

            // Return new and initialized CharModel.
            return charModel;

        }


        /// <summary>
        /// Calls FFLDeleteCharModel, deleting a CharModel's shapes and texture handles.
        /// </summary>
        /// <param name="charModel">CharModel reference.</param>
        public static unsafe void DeleteCharModel(ref FFLCharModel charModel)
        {
            fixed (FFLCharModel* pCharModel = &charModel)
            {
                FFL.DeleteCharModel(pCharModel);
            }
        }

        private const FFLFontRegion _fontRegion = FFLFontRegion.FFL_FONT_REGION_JP_US_EU; // Default font region used even internally.

        /// <summary>
        /// Calls FFLInitRes() to initialize FFL and its resource. This is needed in order to do anything else with the library.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Indicates that the resource file does not exist.</exception>
        /// <exception cref="ArgumentException">Indicates that the file's size is not valid.</exception>
        /// <exception cref="FFLResultException">Indicates FFL returned some error during initialization.</exception>
        /// <exception cref="BrokenInitRes">Indicates that the resource header/signature is invalid.</exception>
        public static void InitializeFFL(string resourceHighPath)
        {
            // Local ResourceDesc's size and pointers should be initialized to 0 (this would need to be done manually in C/C++)

            // Read resource data into byte array.
            byte[] fileData = File.ReadAllBytes(resourceHighPath); // Will throw exception if it does not exist.

            // Ensure it is not empty.
            if (fileData.Length <= 0)
            {
                throw new ArgumentException("The resource file is empty.");
            }
            // Make sure that the file in question is large enough to be an FFL resource.
            else if (fileData.Length <= _resourceHeaderDefaultSize)
            {
                throw new ArgumentException("The resource file provided is not large enough.");
            }

            //fileData[0] = (byte)'S'; // Tamper with file data to force FFLSharp.BrokenInitRes

            // Allocate unmanaged pointer.
            IntPtr fileDataPtr = Marshal.AllocHGlobal(fileData.Length);
            // Copy file data into unmanaged pointer.
            Marshal.Copy(fileData, 0, fileDataPtr, fileData.Length);

            // Always set the high resource descriptor. Give it the unmanaged pointer.
            SetResourceDesc(FFLResourceType.FFL_RESOURCE_TYPE_HIGH, fileDataPtr, (uint)fileData.Length);

            FFLResult result;
            unsafe
            {
                fixed (FFLResourceDesc* pResourceDesc = &_resourceDesc)
                {
                    // This is the call that may or may not crash if
                    // FFL cannot be loaded or it somehow crashes/segfaults.
                    result = FFL.InitRes(_fontRegion, pResourceDesc);
                }
            }

            // Prepare for an invalid result by freeing resources.
            if (result != FFLResult.FFL_RESULT_OK)
                // Free the file pointers.
                DisposeResourceDesc();

            // Throw a more specific exception for this result.
            if (result == FFLResult.FFL_RESULT_FILE_INVALID)
                throw new BrokenInitRes(result);

            // Handle all other results.
            FFLResultException.HandleResult(result);

            // This should always return 1 in any circumstance after this.
            Debug.Assert(FFL.IsAvailable() == 1);
            IsAvailable = true; // Assumed to be true.

            FFL.InitResGPUStep(); // CanInitCharModel will fail if you don't do this

            // _resourceDesc.pData's pointers NEED to be freed when done with FFL. Dispose() will do this.
        }

        public static void InitializeFFL()
        {
            InitializeFFL(_resourceHighFilename);
        }

        /// <summary>
        /// Set the static FFLResourceDesc's size and pointer for one resource.
        /// </summary>
        /// <param name="resourceType">The resource type of the ResourceDesc to set.</param>
        /// <param name="dataPtr">Pointer to the data. Caller must free this after exiting FFL.</param>
        /// <param name="size">Size of the data.</param>
        private static unsafe void SetResourceDesc(FFLResourceType resourceType, IntPtr dataPtr, uint size)
        {
            // Set the size field
            _resourceDesc.size[(int)resourceType] = size;
            _resourceDesc.pData[(int)resourceType] = (void*)dataPtr;
        }

        public static unsafe void DisposeResourceDesc()
        {
            // Iterate through resourceDesc, which is an array representing all FFLResourceTypes.
            for (FFLResourceType i = 0; i < FFLResourceType.FFL_RESOURCE_TYPE_MAX; i++)
            {
                if (_resourceDesc.pData[(int)i] == null)
                    continue; // Skip this resource, it is not allocated.
                Marshal.FreeHGlobal((IntPtr)_resourceDesc.pData[(int)i]); // Free unmanaged pointer.

                // Reset back to zeroes.
                _resourceDesc.pData[(int)i] = null;
                _resourceDesc.size[(int)i] = 0;
            }
        }

        /// <summary>
        /// Frees resource data buffer and calls FFLExit().
        /// You actually cannot call this if FFL is not initialized correctly.
        /// </summary>
        public static void Dispose()
        {
            // Call FFLExit to clean up any internal FFL resources.
            FFLResult result = FFL.Exit(); // Call FFLExit to clean up any internal FFL resources.
            FFLResultException.HandleResult(result);

            DisposeResourceDesc(); // Free resources.
        } // void Dispose
    } // class FFLManager

    public static class FFLProperties
    {
        private static float _modelScale = 1.0f; // FFL default scale is 10.0f, so "1" here is really 10.
        /// <summary>
        /// Set/get the FFL model scale. By default, it is 10. To set it to 1.0, use "0.1" here.
        /// </summary>
        public static float ModelScale
        {
            get => _modelScale;
            set
            {
                FFL.SetScale(value); _modelScale = value;
            }
        }

        /// <summary>
        /// Enable/disabling flipping Y in render textures, or "flip UVs".
        /// This means mask and faceline textures. If you are using OpenGL
        /// where Y direction is opposite from other APIs, and you notice
        /// that the face looks "upside down", you need to set this to true.
        /// </summary>
        public static bool TextureFlipY
        {
            // NOTE: No getter because its value is controlled
            // by whether or not RIO_NO_CLIP_CONTROL is defined.
            set
            {
                // Set bool as byte value.
                FFL.SetTextureFlipY(value ? (byte)1 : (byte)0);
            }
        }

        private static bool _normalIsSnorm8_8_8_8 = false;
        /// <summary>
        /// Enable use of 8_8_8_8 format (4 signed normalized bytes) for
        /// normals rather, than 10_10_10_2, which is not supported by some APIs.
        /// </summary>
        public static bool NormalIsSnorm8_8_8_8
        {
            get => _normalIsSnorm8_8_8_8;
            set
            {
                // Set bool as byte value.
                FFL.SetNormalIsSnorm8_8_8_8(value ? (byte)1 : (byte)0); _normalIsSnorm8_8_8_8 = value;
            }
        }

        private static bool _setFrontCullForFlipX = false;
        /// <summary>
        /// This value controls whether or not front face culling
        /// will be used for meshes with flipped X (flipped hair),
        /// otherwise the index buffer will be adjusted to reverse
        /// triangle winding. This is set true for Veldrid.
        /// </summary>
        public static bool FrontCullForFlipX
        {
            get => _setFrontCullForFlipX;
            set
            {
                // Set bool as byte value.
                FFL.SetFrontCullForFlipX(value ? (byte)1 : (byte)0); _setFrontCullForFlipX = value;
            }
        }
    }
} // namespace FFLSharp