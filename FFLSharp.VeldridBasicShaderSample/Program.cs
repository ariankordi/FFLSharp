using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;
using System.Numerics;
using System.Diagnostics;

using FFLSharp.Interop;
using FFLSharp.VeldridRenderer;
using System;
using System.Collections.Generic;
using System.Linq;

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

        static void Main(string[] args)
        {
            #region Veldrid Initialization/Startup

            // Create window and GraphicsDevice.
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
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

            InstantiateFFLAndHelpers(_graphicsDevice); // also initializes charmodel
            //_commandList.PopDebugGroup();

            // List of sample data to cycle through.
            List<byte[]> storeDataList = SampleData.StoreDataSampleList;

            /*
            FFLManager.CreateCharModelFromStoreData(ref charModel,
                storeDataList.First(), _textureManager); // first sample
            */

            // Define initialization parameters for the first CharModel.
            CharModelInitParam initParam = new CharModelInitParam(
                data: storeDataList.First(),
                expressionFlag: CharModelInitParam.MakeExpressionFlag(FFLExpression.FFL_EXPRESSION_NORMAL,
                                                                      FFLExpression.FFL_EXPRESSION_BLINK)
            );
            /*
            FFLCharModel charModel = FFLManager.CreateCharModel(param);
            */

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

            CharModelImpl charModelRenderer = new CharModelImpl(_graphicsDevice, _resourceManager,
                _textureManager, factory, initParam);

            Console.WriteLine($"Initialized with model: {charModelRenderer.CharModel.GetName()}");

            Stopwatch stopwatchBlink = new Stopwatch();
            stopwatchBlink.Start();

            // Rotation variables.
            Stopwatch stopwatchRotate = new Stopwatch();
            stopwatchRotate.Start();

            float rotationSpeed = 1.0f; // Speed of rotation in radians per second.
            float rotationAngle = 0.0f; // Initial rotation angle.

            FFLExpression initialExpression = charModelRenderer.CurrentExpression;
            bool isBlinking = false; // Will be modified by UpdateCharModelBlink.


            int currentStoreDataIndex = 0; // Current model index.

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

                _commandList.ClearColorTarget(0, new RgbaFloat(0.9f, 0.86f, 1.0f, 1.0f)); //0.2f, 0.3f, 0.3f, 1.0f)); // RIO sample color
                _commandList.ClearDepthStencil(1.0f); // Drawing shapes. Need this.
                _commandList.PopDebugGroup();


                // Calculate the delta time (time between frames)
                float deltaTime = (float)stopwatchRotate.Elapsed.TotalSeconds;
                stopwatchRotate.Restart();
                // Update rotation angle based on deltaTime and speed
                rotationAngle += deltaTime * rotationSpeed;
                // Create rotation matrix around the Y-axis
                Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationY(rotationAngle);
                // Apply the rotation to the model matrix
                modelMatrix = rotationMatrix * Matrix4x4.Identity; // You can multiply with other transformations if needed

                // Upon a complete rotation, change the model.
                if (rotationAngle >= (Math.PI * 2))
                {
                    // Update the model index (cycle through the list)
                    currentStoreDataIndex++;

                    // If we reached the end of the list, start from the beginning
                    if (currentStoreDataIndex >= storeDataList.Count)
                        currentStoreDataIndex = 0;

                    // Create a new model from the next instance in the list
                    charModelRenderer?.Dispose();
                    charModelRenderer = new CharModelImpl(_graphicsDevice, _resourceManager,
                        _textureManager, factory, new CharModelInitParam(
                        data: storeDataList[currentStoreDataIndex],
                        expressionFlag: CharModelInitParam.MakeExpressionFlag(
                                        FFLExpression.FFL_EXPRESSION_NORMAL,
                                        FFLExpression.FFL_EXPRESSION_BLINK)));

                    // Print the name of this CharModel.
                    Console.WriteLine($"Switched to model: {charModelRenderer.CharModel.GetName()}");

                    // Reset the rotation angle for the next cycle
                    rotationAngle = 0.0f;
                }

                UpdateCharModelBlink(ref isBlinking, stopwatchBlink, ref charModelRenderer, initialExpression);

                // Update view uniforms and draw the CharModel.
                charModelRenderer.UpdateViewUniforms(modelMatrix, viewMatrix, projectionMatrix);
                charModelRenderer.Draw(_commandList);

                // End() must be called before commands can be submitted for execution.
                _commandList.End();
                _graphicsDevice.SubmitCommands(_commandList);

                // Once commands have been submitted, the rendered image is presented.
                _graphicsDevice.SwapBuffers();
            }
            #endregion

            #region Cleanup
            // Window is exited, dispose resources in this scope.
            charModelRenderer?.Dispose();

            // -> Dispose();
            #endregion
        }

        private static void InstantiateFFLAndHelpers(GraphicsDevice graphicsDevice)
        {
            // Load FFL resource
            FFLManager.InitializeFFL(); // basically calls FFLInitResEx

            // Initialize texture callback handler and register with FFL.
            _textureManager = new TextureManager(graphicsDevice);
            //_textureManager.RegisterCallback(); // Now explicitly specifying it.
            //_textureCallback = _textureManager.GetTextureCallback();

            FFLProperties.ModelScale = 0.1f; // reset FFL scale from 10.0 to 1.0

            // apparently inverted means NOT opengl
            FFLProperties.TextureFlipY = graphicsDevice.BackendType == GraphicsBackend.OpenGL && !graphicsDevice.IsClipSpaceYInverted;

            // Enable use of 8_8_8_8 format for normals rather than 10_10_10_2.
            FFLProperties.NormalIsSnorm8_8_8_8 = true; // Because Veldrid doesn't support 10_10_10_2
        }

        private const int _blinkInterval = 3000; // 3 secs
        private const int _blinkDuration = 80;   // 80ms

        private static void UpdateCharModelBlink(ref bool isBlinking, Stopwatch stopwatch,
            ref CharModelImpl charModelImpl, FFLExpression initialExpression)
        {
            // Get the current time to compare against.
            long currentTime = stopwatch.ElapsedMilliseconds;

            if (currentTime >= _blinkInterval // Check if it's time to blink.
                // Make sure they are not in the blink state right now.
                && charModelImpl.CurrentExpression == initialExpression)
            {
                // Set expression to blink.
                charModelImpl.SetExpression(FFLExpression.FFL_EXPRESSION_BLINK);
                Console.WriteLine($"expression: {FFLExpression.FFL_EXPRESSION_BLINK}");
                isBlinking = true; // Set state to blinking.
                stopwatch.Restart(); // Restart the timer.
            }

            // Check if we need to revert back to normal after 80ms
            else if (isBlinking && currentTime >= _blinkDuration)
            {
                // Set expression back.
                charModelImpl.SetExpression(initialExpression);
                Console.WriteLine($"expression: {initialExpression}");
                isBlinking = false; // Reset state to not blinking anymore.
                stopwatch.Restart(); // Restart the timer.
            }

        }

        public void Dispose()
        {
            // Free texture and resource managers
            _textureManager.Dispose();
            _resourceManager.Dispose();

            FFLManager.Dispose(); // Calls FFLExit and then frees FFL resource.

            _graphicsDevice?.Dispose();
            _commandList?.Dispose();
        }
    }
}
