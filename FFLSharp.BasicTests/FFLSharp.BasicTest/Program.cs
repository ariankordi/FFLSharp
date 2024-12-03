using FFLSharp.Interop;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace FFLSharp.BasicTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            byte isAvailable = FFL.IsAvailable();
            Console.WriteLine($"FFLIsAvailable at launch: {isAvailable}");
            InitializeFFL();
            isAvailable = FFL.IsAvailable();
            Console.WriteLine($"FFLIsAvailable after initialization: {isAvailable}");

            FFLCharModel charModel = new FFLCharModel();
            var result = CreateCharModelFromStoreData(ref charModel, cJasmineStoreData);
            Console.WriteLine($"CreateCharModelFromStoreData result: {(int)result}");


            Console.WriteLine("Running CleanupFFL()");
            CleanupFFL();
        }


        public static readonly byte[] cJasmineStoreData = new byte[96]
        {
            0x03, 0x00, 0x00, 0x40, 0xA0, 0x41, 0x38, 0xC4, 0xA0, 0x84, 0x00, 0x00, 0xDB, 0xB8, 0x87, 0x31,
            0xBE, 0x60, 0x2B, 0x2A, 0x2A, 0x42, 0x00, 0x00, 0x59, 0x2D, 0x4A, 0x00, 0x61, 0x00, 0x73, 0x00,
            0x6D, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x37,
            0x12, 0x10, 0x7B, 0x01, 0x21, 0x6E, 0x43, 0x1C, 0x0D, 0x64, 0xC7, 0x18, 0x00, 0x08, 0x1E, 0x82,
            0x0D, 0x00, 0x30, 0x41, 0xB3, 0x5B, 0x82, 0x6D, 0x00, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x69, 0x00,
            0x67, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x3A
        };

        public static FFLResult CreateCharModelFromStoreData(ref FFLCharModel pCharModel, byte[] pStoreDataBuffer)
        {
            unsafe
            {
                fixed (byte* pBuffer = pStoreDataBuffer)
                {
                    // Create the FFLCharModelSource struct
                    var modelSource = new FFLCharModelSource
                    {
                        dataSource = FFLDataSource.FFL_DATA_SOURCE_STORE_DATA,
                        pBuffer = pBuffer,
                        index = 0
                    };

                    const int expressionFlag = (1 << (int)FFLExpression.FFL_EXPRESSION_NORMAL) |
                                               (1 << (int)FFLExpression.FFL_EXPRESSION_BLINK);

                    var modelDesc = new FFLCharModelDesc
                    {
                        resolution = (FFLResolution)512,
                        expressionFlag = expressionFlag,
                        modelFlag = (uint)FFLModelFlag.FFL_MODEL_FLAG_NORMAL,
                        resourceType = FFLResourceType.FFL_RESOURCE_TYPE_HIGH
                    };

                    Console.WriteLine("Calling FFLInitCharModelCPUStep");
                    FFLResult result = FFL.InitCharModelCPUStep((FFLCharModel*)Unsafe.AsPointer(ref pCharModel),
                                                                &modelSource,
                                                                &modelDesc);

                    if (result != FFLResult.FFL_RESULT_OK)
                    {
                        Console.WriteLine($"FFLInitCharModelCPUStep failed with result: {(int)result}");
                        return result;
                    }

                    return result;
                }
            }
        }



        private static FFLResourceDesc gResourceDesc = new FFLResourceDesc();
        private const string cFFLResourceHighFilename = "./FFLResHigh.dat";

        public static FFLResult InitializeFFL()
        {
            Console.WriteLine("Before FFL initialization");

            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_HIGH, IntPtr.Zero, 0);
            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_MIDDLE, IntPtr.Zero, 0);

            if (!File.Exists(cFFLResourceHighFilename))
            {
                Console.WriteLine($"Error: Cannot open file {cFFLResourceHighFilename}");
                return FFLResult.FFL_RESULT_FS_ERROR;
            }

            byte[] fileData = File.ReadAllBytes(cFFLResourceHighFilename);
            int fileSize = fileData.Length;

            if (fileSize <= 0)
            {
                Console.WriteLine($"Invalid file size for {cFFLResourceHighFilename}");
                return FFLResult.FFL_RESULT_FS_ERROR;
            }

            IntPtr fileDataPtr = Marshal.AllocHGlobal(fileSize);
            Marshal.Copy(fileData, 0, fileDataPtr, fileSize);

            SetResourceData(FFLResourceType.FFL_RESOURCE_TYPE_HIGH, fileDataPtr, (uint)fileSize);

            Console.WriteLine("Calling FFLInitResEx");

            FFLResult result;
            unsafe
            {
                fixed (FFLResourceDesc* pResourceDesc = &gResourceDesc)
                {
                    result = FFL.InitRes(FFLFontRegion.FFL_FONT_REGION_JP_US_EU, pResourceDesc);
                }
            }

            if (result != FFLResult.FFL_RESULT_OK)
            {
                Console.WriteLine($"FFLInitResEx() failed with result: {(int)result}");
                Marshal.FreeHGlobal(fileDataPtr);
                return result;
            }

            if (FFL.IsAvailable() == 0)
            {
                Console.WriteLine("FFL is not available after initialization");
                Marshal.FreeHGlobal(fileDataPtr);
                return FFLResult.FFL_RESULT_ERROR;
            }

            FFL.InitResGPUStep();
            Console.WriteLine("Exiting InitializeFFL()");
            return result;
        }
        private static void SetResourceData(FFLResourceType resourceType, IntPtr dataPtr, uint size)
        {
            unsafe
            {
                int index = (int)resourceType;

                // Set the size field
                gResourceDesc.size[index] = size;

                // Set the pData pointer directly
                if (index == 0)
                {
                    gResourceDesc.pData.e0 = (void*)dataPtr;
                }
                else if (index == 1)
                {
                    gResourceDesc.pData.e1 = (void*)dataPtr;
                }
            }
        }
        public static void DeleteCharModel(FFLCharModel charModel)
        {
            unsafe
            {
                FFL.DeleteCharModel(&charModel);
            }
        }
        public static void CleanupFFL()
        {
            unsafe
            {
                // Free the memory allocated for pData in gResourceDesc
                if (gResourceDesc.pData[0] != null)
                {
                    Marshal.FreeHGlobal((IntPtr)gResourceDesc.pData[0]);
                    gResourceDesc.pData[0] = null;
                }

                if (gResourceDesc.pData[1] != null)
                {
                    Marshal.FreeHGlobal((IntPtr)gResourceDesc.pData[1]);
                    gResourceDesc.pData[1] = null;
                }
            }

            // Call FFLExit to clean up any internal FFL resources
            FFL.Exit();
        }
    }
}