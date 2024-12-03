using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;
using System.Numerics;
using System.Diagnostics;

using FFLSharp.Interop;
using FFLSharp.Veldrid;

namespace FFLSharp.VeldridBasicShaderSample
{
    class Program : IDisposable
    {
        // Window title.
        private const string _windowTitle = "ffl veldrid c# basic draw sample";

        // Veldrid instances.
        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;

        // Texture manager hosting all textures created from FFL.
        private static TextureManager _textureManager;

        // Resource manager including pipelines, layouts, etc.
        private static CharModelResource _resourceManager;

        //private unsafe static FFLTextureCallback* _textureCallback;

        static void Main(string[] args)
        {
            #region Initialize Veldrid

            // Create window and GraphicsDevice.
            GraphicsDeviceOptions options = new(
#if DEBUG
                debug: true,
#else
                debug: false,
#endif
                // using R16_UNorm caused some irregular depth effects around the mask
                swapchainDepthFormat: PixelFormat.D24_UNorm_S8_UInt,
                syncToVerticalBlank: true, // aka V-Sync
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: false,
                preferStandardClipSpaceYDirection: true); // doesn't do anything on OpenGL

            GraphicsBackend backend =
                //GraphicsBackend.Vulkan;
                VeldridStartup.GetPlatformDefaultBackend();

            // Create Sdl2Window and GraphicsDevice
            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCI: new WindowCreateInfo(
                    x: 100,
                    y: 100,
                    windowWidth: 800,
                    windowHeight: 600,
                    windowInitialState: WindowState.Normal, // fullscreen, etc
                    windowTitle: _windowTitle
                ),
                deviceOptions: options,
                preferredBackend: backend,
                window: out Sdl2Window window,
                gd: out GraphicsDevice graphicsDevice);
            _graphicsDevice = graphicsDevice; // assign to class instance

            Debug.Assert(_graphicsDevice != null);

            // Create ResourceFactory and CommandList.
            ResourceFactory factory = _graphicsDevice.ResourceFactory;
            _commandList = factory.CreateCommandList();
            #endregion

            //_commandList.PushDebugGroup("FFLInitCharModelCPUStep Uploading Textures"); // doesn't even appear
            FFLCharModel charModel = new();
            InstantiateFFLAndHelpers(_graphicsDevice, ref charModel); // also initializes charmodel
            //_commandList.PopDebugGroup();

            _resourceManager = new CharModelResource(_graphicsDevice);

            Matrix4x4 modelMatrix = Matrix4x4.Identity;
            // Make projection matrix, calculate the aspect ratio
            float aspect = (float)_graphicsDevice.SwapchainFramebuffer.Width / (float)_graphicsDevice.SwapchainFramebuffer.Height;
            Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView: 0.26179939f, // 15 degrees = 0.26179939 radians
                aspectRatio: aspect, nearPlaneDistance: 1.0f, farPlaneDistance: 1000.0f);
            // Make view matrix from LookAt camera.
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(
                cameraPosition: new Vector3(0.0f, 3.70f, 38.0f),
                cameraTarget: new Vector3(0.0f, /*3.70f*/3.40f, 0.0f),
                cameraUpVector: new Vector3(0.0f, 1.0f, 0.0f)
                // ^^ From nn::mii::VariableIconBody::StoreCameraMatrix but modified to be 1x scale.
            );

            CharModelImpl charModelGpuBuffer = new(_graphicsDevice, _resourceManager,
                _textureManager, factory, /*_commandList,*/ ref charModel);

            Stopwatch _stopwatch = new();
            _stopwatch.Start();

            Stopwatch _stopwatch2 = new();
            _stopwatch2.Start();

            // Rotation variables
            float rotationSpeed = 1.0f; // Speed of rotation in radians per second
            float rotationAngle = 0.0f; // Initial rotation angle

            long blinkEndTime = 0;
            FFLExpression currentExpression = FFLExpression.FFL_EXPRESSION_NORMAL;




            // Assuming storeDataList is your list of StoreData objects
            List<byte[]> storeDataList = new((new[] { FFLHelpers.JasmineStoreData, FFLHelpers.BroStoreData }));

            // This will keep track of the current model index
            int currentStoreDataIndex = 0;

            // A stopwatch or counter to track how long to hold each model (optional for smooth transitions)
            Stopwatch modelSwitchTimer = new();
            modelSwitchTimer.Start();

            #region Draw Loop
            while (window.Exists)
            {
                window.PumpEvents();
                // Break out of the loop if window is exiting
                if (!window.Exists) { break; } // Jumps to after the loop to clean up.

                // Begin() must be called before commands can be issued.
                _commandList.Begin();
                _commandList.PushDebugGroup("Pre-Draw Clear Color/Depth");

                // We want to render directly to the output window.
                _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

                _commandList.ClearColorTarget(0, new(0.9f, 0.86f, 1.0f, 1.0f)); //0.2f, 0.3f, 0.3f, 1.0f)); // RIO sample color
                _commandList.ClearDepthStencil(1.0f); // Drawing shapes. Need this.
                _commandList.PopDebugGroup();


                // Calculate the delta time (time between frames)
                float deltaTime = (float)_stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Restart();
                // Update rotation angle based on deltaTime and speed
                rotationAngle += deltaTime * rotationSpeed;
                // Create rotation matrix around the Y-axis
                Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationY(rotationAngle);
                // Apply the rotation to the model matrix
                modelMatrix = rotationMatrix * Matrix4x4.Identity; // You can multiply with other transformations if needed



                // Example of a timer or a condition to change models (every 3 seconds, for example)
                if (rotationAngle >= (Math.PI * 2)) // on a complete rotation?
                {
                    // Update the model index (cycle through the list)
                    currentStoreDataIndex++;

                    // If we reached the end of the list, start from the beginning
                    if (currentStoreDataIndex >= storeDataList.Count)
                    {
                        currentStoreDataIndex = 0;
                    }

                    // Create a new model from the next store data in the list
                    unsafe
                    {
                        FFLHelpers.CreateCharModelFromStoreData(ref charModel, storeDataList[currentStoreDataIndex], _textureManager.GetTextureCallback());
                    }
                    charModelGpuBuffer?.Dispose();
                    charModelGpuBuffer = new CharModelImpl(_graphicsDevice, _resourceManager, _textureManager, factory, ref charModel);
                    charModelGpuBuffer.UpdateViewUniforms(modelMatrix, viewMatrix, projectionMatrix);

                    // Restart the timer for the next cycle
                    modelSwitchTimer.Restart();
                    rotationAngle = 0.0f;
                }
                else
                    charModelGpuBuffer.UpdateViewUniforms(modelMatrix, viewMatrix, projectionMatrix);


                long currentTime = _stopwatch2.ElapsedMilliseconds;

                // Check if 3 seconds have passed
                if (currentTime >= 3000)
                {
                    // Blink for 80ms
                    if (currentExpression == FFLExpression.FFL_EXPRESSION_NORMAL)
                    {
                        charModelGpuBuffer.SetExpression(FFLExpression.FFL_EXPRESSION_BLINK);
                        currentExpression = FFLExpression.FFL_EXPRESSION_BLINK;
                        Console.WriteLine("blink");
                        blinkEndTime = currentTime + 80; // Blink will last for 80ms
                    }
                }

                // Check if we need to revert back to normal after 80ms
                if (currentExpression == FFLExpression.FFL_EXPRESSION_BLINK && currentTime >= blinkEndTime)
                {
                    charModelGpuBuffer.SetExpression(FFLExpression.FFL_EXPRESSION_NORMAL);
                    currentExpression = FFLExpression.FFL_EXPRESSION_NORMAL;
                    Console.WriteLine("unblink");
                    _stopwatch2.Restart();
                }

                charModelGpuBuffer.Draw(_commandList);

                // End() must be called before commands can be submitted for execution.
                _commandList.End();
                _graphicsDevice.SubmitCommands(_commandList);

                // Once commands have been submitted, the rendered image can be presented to the application window.
                _graphicsDevice.SwapBuffers();
            }
            #endregion

            #region Cleanup
            // Window is exited, dispose resources in this scope.
            charModelGpuBuffer?.Dispose();

            // De-initialize FFL: delete all CharModels and call FFLExit.
            //FFLHelpers.DeleteCharModel(ref charModel); // Deleted by CharModelRenderer
            FFLHelpers.CleanupFFL(); // Calls FFLExit and then frees FFL resource.

            // -> Dispose();
            #endregion
        }

        private static unsafe void InstantiateFFLAndHelpers(GraphicsDevice graphicsDevice, ref FFLCharModel charModel)
        {
            // Load FFL resource
            FFLHelpers.InitializeFFL(); // basically calls FFLInitResEx

            // Initialize texture callback handler and register with FFL.
            _textureManager = new(graphicsDevice);
            //_textureManager.RegisterCallback(); // Now explicitly specifying it.
            //_textureCallback = _textureManager.GetTextureCallback();

            FFL.SetScale(0.1f); // reset FFL scale from 10.0 to 1.0
            // Accomodate texture flipping if running on OpenGL.
            unsafe
            {
                // apparently inverted means NOT opengl
                bool enableTextureFlipY = graphicsDevice.BackendType == GraphicsBackend.OpenGL && !graphicsDevice.IsClipSpaceYInverted;
                FFL.SetTextureFlipY(*(byte*)&enableTextureFlipY); // flips Y in mask/faceline textures
            }
            // Enable use of 8_8_8_8 format for normals rather than 10_10_10_2.
            FFL.SetNormalIsSnorm8_8_8_8(1); // Because Veldrid doesn't support 10_10_10_2

            // FFLInitCharModelCPUStep:
            FFLResult result = FFLHelpers.CreateCharModelFromStoreData(ref charModel,
                FFLHelpers.JasmineStoreData, _textureManager.GetTextureCallback());
        }

        public void Dispose()
        {
            // Free texture and resource managers
            _textureManager.Dispose();
            _resourceManager.Dispose();

            _graphicsDevice?.Dispose();
            _commandList?.Dispose();
        }
    }
}
