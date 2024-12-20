using System;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using FFLSharp.Interop;
using FFLSharp.VeldridRenderer;
using Veldrid.OpenGLBinding;
using Veldrid.Sdl2;

namespace FFLSharp.VeldridCreateTGASample
{
    class Program
    {
        // Default resolution if not specified
        private const int DefaultResolution = 800;

        // Entry point
        static void Main(string[] args)
        {
            string ffsdPath = string.Empty;
            int resolution = DefaultResolution;

            // Parse arguments
            if (args.Length == 0)
            {
                ShowHelp();
                Console.Write("Enter the path to the .ffsd file: ");
                ffsdPath = Console.ReadLine()?.Trim('"') ?? string.Empty;

                if (string.IsNullOrWhiteSpace(ffsdPath))
                {
                    Console.WriteLine("No file path provided. Exiting.");
                    return;
                }
            }
            else
            {
                ffsdPath = args[0].Trim('"');
                if (args.Length >= 2 && int.TryParse(args[1], out int res))
                {
                    resolution = res;
                }
                else if (args.Length >= 2)
                {
                    Console.WriteLine("Invalid resolution argument. Using default resolution.");
                }
            }

            if (!File.Exists(ffsdPath))
            {
                Console.WriteLine($"File not found: {ffsdPath}");
                return;
            }

            // Initialize Veldrid for offscreen rendering
            GraphicsDevice graphicsDevice = null;
            CommandList commandList = null;
            try
            {
                var options = new GraphicsDeviceOptions(

#if DEBUG
                    debug: true,
#else
                    debug: false,
#endif
                    swapchainDepthFormat: PixelFormat.D24_UNorm_S8_UInt,
                    syncToVerticalBlank: false,
                    resourceBindingModel: ResourceBindingModel.Improved,
                    preferDepthRangeZeroToOne: false,
                    preferStandardClipSpaceYDirection: true
                );

                // Choose backend (Vulkan, Direct3D11, etc.)
                GraphicsBackend backend = GraphicsBackend.OpenGL;
                                          //VeldridStartup.GetPlatformDefaultBackend();

                // Create a dummy window for context (required by some backends)
                var windowCI = new WindowCreateInfo(
                    x: 100,
                    y: 100,
                    windowWidth: resolution,
                    windowHeight: resolution,
                    windowInitialState: WindowState.Hidden, // Hidden window
                    windowTitle: ""
                );

                // Create a framebuffer with the specified resolution
                Framebuffer framebuffer = CreateOffscreenFramebuffer(resolution, resolution, graphicsDevice, backend, out graphicsDevice);

                // Create ResourceFactory and CommandList
                ResourceFactory factory = graphicsDevice.ResourceFactory;
                commandList = factory.CreateCommandList();

                // Initialize FFL and helpers
                TextureManager textureManager = InitializeFFL(graphicsDevice);

                // Load StoreData
                byte[] storeData = File.ReadAllBytes(ffsdPath);
                CharModelInitParam initParam = new CharModelInitParam(
                    data: storeData
                );

                // Initialize Pipeline Provider
                BasicShaderPipelineProvider pipelineProvider = new BasicShaderPipelineProvider(graphicsDevice);

                // Create CharModelRenderer
                CharModelRenderer charModelRenderer = new CharModelRenderer(
                    graphicsDevice,
                    pipelineProvider,
                    textureManager,
                    factory,
                    initParam
                );

                // Print model and creator names
                Console.WriteLine($"Model Name: {charModelRenderer.CharModel.GetName()}");
                string creatorName = charModelRenderer.CharModel.GetCreatorName();
                if (!string.IsNullOrWhiteSpace(creatorName))
                {
                    Console.WriteLine($"Creator Name: {creatorName}");
                }

                // Set up transformation matrices
                Matrix4x4 modelMatrix = Matrix4x4.Identity;
                float aspect = (float)resolution / (float)resolution;
                float fovy = (float)Math.Atan2(43.2f / aspect, 500.0f) / 0.5f;
                Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                    fieldOfView: fovy, // RFLiMakeIcon
                    aspectRatio: aspect,
                    nearPlaneDistance: 500.0f,
                    farPlaneDistance: 700.0f
                );

                // NOTE: Model scale is currently the FFL default,
                // which is 10.0. The matrices account for that.

                Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(
                    cameraPosition: new Vector3(0.0f, 34.5f, 600.0f),
                    cameraTarget: new Vector3(0.0f, 34.5f, 0.0f),
                    cameraUpVector: new Vector3(0.0f, 1.0f, 0.0f)
                );

                // Update view uniforms
                charModelRenderer.UpdateViewUniforms(modelMatrix, viewMatrix, projectionMatrix);

                // Begin command recording
                commandList.Begin();

                // Set framebuffer
                commandList.SetFramebuffer(framebuffer);

                // Clear color and depth
                commandList.ClearColorTarget(0, RgbaFloat.Clear);//new RgbaFloat(0.9f, 0.86f, 1.0f, 1.0f));
                commandList.ClearDepthStencil(1.0f);

                // Draw the character model
                charModelRenderer.Draw(commandList);

                // End command recording
                commandList.End();

                // Submit commands
                graphicsDevice.SubmitCommands(commandList);
                graphicsDevice.WaitForIdle();

                // Read pixels from framebuffer
                Texture snapshot = graphicsDevice.SwapchainFramebuffer.ColorTargets[0].Target;
                // Alternatively, create a snapshot texture if necessary

                // Read the pixels
                var pixels = ReadPixels(graphicsDevice, framebuffer.ColorTargets[0].Target,
                    factory, graphicsDevice.BackendType);

                // Save to TGA
                string outputPath = Path.ChangeExtension(ffsdPath, ".tga");
                bool flipY = (backend == GraphicsBackend.OpenGL && !graphicsDevice.IsClipSpaceYInverted);

                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    TgaSerializer.Serialize(pixels, resolution, resolution, fs, flipY);
                }

                Console.WriteLine($"Image saved to {outputPath}");

                // Cleanup
                charModelRenderer.Dispose();
                pipelineProvider.Dispose();
                textureManager.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Dispose graphics resources
                graphicsDevice?.Dispose();
                commandList?.Dispose();
            }
        }


        /// <summary>
        /// Displays help information.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {Environment.CommandLine} <path_to_ffsd> [resolution]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <path_to_ffsd>    Path to the .ffsd FFLStoreData file.");
            Console.WriteLine("  [resolution]      Optional. One dimension for square image (default: 800).");
            Console.WriteLine();
            Console.WriteLine("If no arguments are provided, the program will prompt for input.");
        }

        /// <summary>
        /// Creates an offscreen framebuffer.
        /// </summary>
        private static Framebuffer CreateOffscreenFramebuffer(int width, int height, GraphicsDevice graphicsDevice, GraphicsBackend backend, out GraphicsDevice gd)
        {
            // Create GraphicsDevice with a hidden window
            var windowCI = new WindowCreateInfo(
                x: 100,
                y: 100,
                windowWidth: width,
                windowHeight: height,
                windowInitialState: WindowState.Hidden,
                windowTitle: "Offscreen"
            );

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
#if DEBUG
                debug: true,
#else
                debug: false,
#endif
                swapchainDepthFormat: PixelFormat.D24_UNorm_S8_UInt,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: false,
                preferStandardClipSpaceYDirection: true // Flip Y
            );

            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCI,
                options,
                backend,
                out Sdl2Window window,
                out GraphicsDevice graphicsDeviceOut
            );

            gd = graphicsDeviceOut;

            // Create a BGRA texture for the framebuffer
            Texture texture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                mipLevels: 1,
                arrayLayers: 1,
                PixelFormat.B8_G8_R8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled
            ));

            // Create a depth texture
            Texture depthTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                mipLevels: 1,
                arrayLayers: 1,
                PixelFormat.D24_UNorm_S8_UInt,
                TextureUsage.DepthStencil
            ));

            // Create framebuffer
            Framebuffer fb = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
                depthTexture,
                texture
            ));

            return fb;
        }

        /// <summary>
        /// Initializes FFL and related helpers.
        /// </summary>
        private static TextureManager InitializeFFL(GraphicsDevice graphicsDevice)
        {
            // Initialize FFL
            FFLManager.InitializeFFL();

            // Initialize texture manager
            TextureManager textureManager = new TextureManager(graphicsDevice);

            // Set FFL properties
            //FFLProperties.ModelScale = 1.0f;
            FFLProperties.TextureFlipY = graphicsDevice.BackendType == GraphicsBackend.OpenGL && !graphicsDevice.IsClipSpaceYInverted;
            FFLProperties.NormalIsSnorm8_8_8_8 = true; // Veldrid compatibility

            return textureManager;
        }


        public static unsafe byte[] ReadPixels(GraphicsDevice device, Texture texture, ResourceFactory factory, GraphicsBackend backend)
        {
            uint width = texture.Width;
            uint height = texture.Height;
            byte[] pixelData;

            switch (backend)
            {
                // Special case for OpenGL
                case GraphicsBackend.OpenGL:
                    {
                        // Allocate a buffer for pixel data
                        pixelData = new byte[width * height * 4]; // Assuming RGBA format (4 bytes per pixel)

                        // Get OpenGL info and read pixels directly
                        var info = device.GetOpenGLInfo();
                        info.ExecuteOnGLThread(() =>
                        {
                            fixed (byte* data = pixelData)
                            {
                                OpenGLNative.glReadPixels(0, 0, width, height, GLPixelFormat.Bgra, GLPixelType.UnsignedByte, data);
                            }
                        });

                        return pixelData;
                    }

                // Default case for other backends (Direct3D11, Vulkan, etc.)
                default:
                    {
                        // Create a staging texture
                        using var staging = factory.CreateTexture(TextureDescription.Texture2D(
                            width, height, 1, 1, texture.Format, TextureUsage.Staging));
                        using var commands = factory.CreateCommandList();
                        using var fence = factory.CreateFence(false);

                        // Copy data from the framebuffer/texture to the staging texture
                        commands.Begin();
                        commands.CopyTexture(texture, staging);
                        commands.End();

                        // Submit commands and wait for completion
                        device.SubmitCommands(commands, fence);
                        device.WaitForFence(fence);

                        // Map the staging texture for reading
                        MappedResource resource = device.Map(staging, MapMode.Read);

                        // Create a span to access the pixel data
                        pixelData = new byte[resource.SizeInBytes];
                        fixed (byte* dataPtr = pixelData)
                        {
                            // Create a span pointing to the destination array
                            Span<byte> destinationSpan = new Span<byte>(dataPtr, pixelData.Length);
                            // Create a span pointing to the source mapped resource
                            Span<byte> sourceSpan = new Span<byte>(resource.Data.ToPointer(), (int)resource.SizeInBytes);

                            // Copy the data from source to destination
                            sourceSpan.CopyTo(destinationSpan);
                        }

                        // Unmap the staging texture
                        device.Unmap(staging);

                        return pixelData;
                    }
            }
        }

    }

    /// <summary>
    /// Custom TGA serializer.
    /// </summary>
    public static class TgaSerializer
    {
        /// <summary>
        /// Serializes pixel data to a TGA file.
        /// </summary>
        /// <param name="pixels">Pixel data in BGRA format.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="stream">Output stream.</param>
        public static void Serialize(byte[] pixels, int width, int height, Stream stream, bool flipY)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // TGA Header
                writer.Write((byte)0); // ID length
                writer.Write((byte)0); // Color map type
                writer.Write((byte)2); // Image type: Uncompressed True-Color Image

                // Color map specification
                writer.Write((short)0); // First entry index
                writer.Write((short)0); // Color map length
                writer.Write((byte)0);  // Color map entry size

                // Image specification
                writer.Write((short)0); // X-origin
                writer.Write((short)0); // Y-origin
                writer.Write((short)width); // Image width
                writer.Write((short)height); // Image height
                writer.Write((byte)32); // Pixel depth (BGRA)
                byte imageDescriptor = flipY ? (byte)0x08 : (byte)0x20; // Alpha bits set to 8.
                writer.Write(imageDescriptor);

                // Write pixel data
                writer.Write(pixels);
            }
        }
    }

}
