using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;

using FFLSharp.Interop;
using static FFLSharp.BasicTest.Program; // CreateCharModelFromStoreData
using System.Runtime.CompilerServices; // for InitializeFFL, CleanupFFL

using FFLSharp.TextureTest; // TextureCallbackHandler
using Veldrid.SPIRV;

namespace FFLSharp.ShaderTextureTest
{
    class Program : IDisposable
    {
        private const string windowTitle = "ffl veldrid c# basic shader sample";

        private static TextureCallbackHandler? _textureCallbackHandler;
        private static GCHandle _textureCallbackHandlerHandle;
        private static GCHandle _textureCallbackHandle;

        private static ShaderCallbackHandler? _shaderCallbackHandler;
        private static GCHandle _shaderCallbackHandlerHandle;
        private static FFLShaderCallback _shaderCallback;
        private static GCHandle _shaderCallbackHandle;


        // MVP
        private static Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private static Matrix4x4 _viewMatrix;
        private static Matrix4x4 _projectionMatrix;

        // For faceline and mask textures
        private static Framebuffer _facelineFramebuffer;
        private static Texture FacelineTexture;

        private static Framebuffer[] _maskFramebuffers = new Framebuffer[(int)FFLExpression.FFL_EXPRESSION_MAX];
        private static Texture[] MaskTextures = new Texture[(int)FFLExpression.FFL_EXPRESSION_MAX];

        static void Main(string[] args)
        {

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: true,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);

            GraphicsBackend backend = GraphicsBackend.Vulkan; //VeldridStartup.GetPlatformDefaultBackend();

            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, 800, 600, WindowState.Normal, windowTitle),
                options,
                backend,
                out Sdl2Window _window,
                out GraphicsDevice gd);

            // initialize ffflllllll
            InitializeFFL(); // basically calls FFLInitResEx

            // initialize texture callback handler
            _textureCallbackHandler = new TextureCallbackHandler(gd);
            _textureCallbackHandlerHandle = GCHandle.Alloc(_textureCallbackHandler);
            unsafe
            {
                FFLTextureCallback textureCallback = new FFLTextureCallback
                {
                    pObj = (void*)GCHandle.ToIntPtr(_textureCallbackHandlerHandle),
                    pCreateFunc = &TextureCallbackHandler.CreateTextureCallback,
                    pDeleteFunc = &TextureCallbackHandler.DeleteTextureCallback
                };
                _textureCallbackHandle = GCHandle.Alloc(textureCallback, GCHandleType.Pinned);

                FFL.SetTextureCallback(&textureCallback);
            }

            ResourceFactory factory = gd.ResourceFactory;
            CommandList cl = factory.CreateCommandList();

            // ==== Triangle Vertices ====
/*
            // Define vertex structure
            VertexPositionColor[] vertices =
            {
                new VertexPositionColor(new Vector2(0.0f,  0.5f),  RgbaFloat.Red),
                new VertexPositionColor(new Vector2(0.5f, -0.5f),  RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-0.5f, -0.5f), RgbaFloat.Blue),
            };

            // Create vertex buffer
            uint sizeOfVertex = (uint)Marshal.SizeOf<VertexPositionColor>();
            BufferDescription vbDescription = new BufferDescription(
                sizeOfVertex * (uint)vertices.Length,
                BufferUsage.VertexBuffer);
            DeviceBuffer vertexBuffer = factory.CreateBuffer(vbDescription);
            gd.UpdateBuffer(vertexBuffer, 0, vertices);

            // Create shaders
            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(
                    @"#version 450
                      layout(location = 0) in vec2 Position;
                      layout(location = 1) in vec4 Color;

                      layout(location = 0) out vec4 fsin_Color;
                      void main()
                      {
                          gl_Position = vec4(Position, 0.0, 1.0);
                          fsin_Color = Color;
                      }"),
                    "main"),

                new ShaderDescription(
                    ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(
                    @"#version 450
                      layout(location = 0) in vec4 fsin_Color;

                      layout(location = 0) out vec4 fsout_Color;

                      void main()
                      {
                          fsout_Color = fsin_Color;
                      }"),
                    "main")
            );

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.CullNone,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription(
                    new[]
                    {
                        // Define vertex layout
                        new VertexLayoutDescription(
                            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4))
                    },
                    shaders: shaders),
                ResourceLayouts = Array.Empty<ResourceLayout>(), // No resources used
                Outputs = gd.SwapchainFramebuffer.OutputDescription
            };

            Pipeline pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
*/
            // ==== FFL Shader Callback ====

            // Set up the shader callback
            //cl.Begin();
            //cl.PushDebugGroup("Upload Textures for FFLInitCharModelCPUStep");

            _shaderCallbackHandler = new ShaderCallbackHandler(gd, cl, _textureCallbackHandler);
            _shaderCallbackHandlerHandle = GCHandle.Alloc(_shaderCallbackHandler);
            unsafe
            {
                _shaderCallback = new FFLShaderCallback
                {
                    pObj = (void*)GCHandle.ToIntPtr(_shaderCallbackHandlerHandle),
                    /* Not necessary if not calling FFLInitCharModelGPUStep, which we can't.
                    pSetMatrixFunc = &ShaderCallbackHandler.SetMatrixCallback;
                    pApplyAlphaTestFunc = &ShaderCallbackHandler.ApplyAlphaTestCallback;
                    */
                    pDrawFunc = &ShaderCallbackHandler.DrawCallback
                };
                _shaderCallbackHandle = GCHandle.Alloc(_shaderCallback, GCHandleType.Pinned);
                // Register the shader callback with FFL
                fixed (FFLShaderCallback* pCallback = &_shaderCallback)
                {
                    FFL.SetShaderCallback(pCallback);
                }
            }

            FFL.SetScale(0.1f); // reset FFL scale from 10.0 to 1.0
            unsafe
            {
                bool enableTextureFlipY = !gd.IsClipSpaceYInverted; // apparently inverted means NOT opengl
                FFL.SetTextureFlipY(*(byte*)&enableTextureFlipY); // flips Y in mask/faceline textures
            }

            FFLCharModel charModel = new FFLCharModel();

            FFLResult result = CreateCharModelFromStoreData(ref charModel, cJasmineStoreData); // FFLInitCharModelCPUStep
            Console.WriteLine($"CreateCharModelFromStoreData result: {(int)result}");

            //cl.End(); // ?????? is this even needed

            uint textureResolution = 0; // set in the unsafe block below

            // set textureResolution
            unsafe
            {
                // const color for theeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
                /*
                FFLDrawParam* pFaceDrawParam = FFL.GetDrawParamOpaFaceline(&charModel); // usually returned as const
                FFLColor pFacelineColor = FFL.GetFacelineColor(((FFLiCharModel*)&charModel)->charInfo.parts.facelineColor);
                pFaceDrawParam->modulateParam.pColorR = &pFacelineColor;
                pFaceDrawParam->modulateParam.mode = FFLModulateMode.FFL_MODULATE_MODE_CONSTANT;
                */
                FFLiCharModel* pCharModel = (FFLiCharModel*)&charModel;
                Console.WriteLine(pCharModel->drawParam[(int)FFLiShapeType.FFLI_SHAPE_TYPE_XLU_MASK]);

                textureResolution = (uint)(pCharModel->charModelDesc.resolution & FFLResolution.FFL_RESOLUTION_MASK);
            }

            Console.WriteLine($"Resolution for render textures: {textureResolution}");

            // just define this for   painless
            const int expression = (int)FFLExpression.FFL_EXPRESSION_NORMAL;

            // Get the swapchain's color format.
            PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;

            // create mask and faceline framebuffers and textures

            uint textureResolutionHalf = textureResolution / 2; // faceline texture is half width

            // Create a render target texture with the swapchain format.
            FacelineTexture = factory.CreateTexture(
                TextureDescription.Texture2D(
                    textureResolutionHalf, textureResolution, 1, 1, // Width, Height, MipLevels, ArrayLayers
                    swapchainFormat,                                   // Use the same format as the swapchain
                    TextureUsage.RenderTarget | TextureUsage.Sampled));  // Enable rendering to and sampling from it


            // Create a framebuffer with the render target texture.
            _facelineFramebuffer = factory.CreateFramebuffer(
                new FramebufferDescription(null, FacelineTexture));

            MaskTextures[expression] = factory.CreateTexture(
                TextureDescription.Texture2D(
                    textureResolution, textureResolution, 1, 1,    // Width, Height, MipLevels, ArrayLayers
                    swapchainFormat,                                  // Use the same format as the swapchain
                    TextureUsage.RenderTarget | TextureUsage.Sampled)); // Enable rendering to and sampling from it
            _maskFramebuffers[expression] = factory.CreateFramebuffer(
                new FramebufferDescription(null, MaskTextures[expression]));

            UIntPtr faceTextureHandle = UIntPtr.Zero; // TODO: JUST HERE SO THAT IT CAN BE FREEEEEED
            UIntPtr maskTextureHandle = UIntPtr.Zero;

            cl.Begin(); // begin command list right now, sure bro

            cl.PushDebugGroup("Draw Faceline & Masks");

            // uhhh guess what we are rendering the mask texture RIGHT HERE!!!!!!!!!!
            unsafe
            {
                FFLiCharModel* pCharModel = (FFLiCharModel*)&charModel;
                void** ppFacelineTexture2D = (void**)&pCharModel->facelineRenderTexture; // HACK: FFLiRenderTexture = FFLTexture**

                //FFL.InitCharModelGPUStep((FFLCharModel*)Unsafe.AsPointer(ref charModel));
                fixed (FFLShaderCallback* pCallback = &_shaderCallback)
                {
                    FFLiTextureTempObject* pTmpObject = ((FFLiCharModel*)&charModel)->pTextureTempObject;

                    if (*ppFacelineTexture2D != null) // should we draw the faceline texture?
                    {
                        cl.PushDebugGroup("Draw Faceline Texture");

                        _shaderCallbackHandler.PipelineMode = ShaderCallbackHandler.PipelineModeType.FacelineTexturePipeline;

                        cl.SetFramebuffer(_facelineFramebuffer);

                        FFLColor pFacelineColor = FFL.GetFacelineColor(pCharModel->charInfo.parts.facelineColor);
                        cl.ClearColorTarget(0, new RgbaFloat(pFacelineColor.r, pFacelineColor.g, pFacelineColor.b, pFacelineColor.a));

//cl.End();
//_shaderCallbackHandler.CurrentFramebuffer = _facelineFramebuffer;

                        FFLiFacelineTextureTempObject* pFaceTmpObject = &pTmpObject->facelineTexture;
                        FFL.iDrawFacelineTexture(pFaceTmpObject, &pCallback);

                        cl.PopDebugGroup();
                    }


                    FFLiMaskTexturesTempObject* pMaskTmpObject = &pTmpObject->maskTextures;
                    FFL.iInvalidatePartsTextures(&pMaskTmpObject->partsTextures); // before looping at ALL

                    // begin rendering to this mask texture
                    FFL.iInvalidateRawMask(pMaskTmpObject->pRawMaskDrawParam[expression]); // after verifying thisis supposed to be drawn but before ANY drawing

                    cl.PushDebugGroup($"Draw Mask Texture Expression {expression}");

                    _shaderCallbackHandler.PipelineMode = ShaderCallbackHandler.PipelineModeType.MaskTexturePipeline;
                    cl.SetFramebuffer(_maskFramebuffers[expression]); // switch to framebuffer
                    cl.ClearColorTarget(0, RgbaFloat.Clear); // transparent for mask
                    
//cl.End();
//_shaderCallbackHandler.CurrentFramebuffer = _maskFramebuffers[expression];
                    FFLiRawMaskDrawParam* pDrawParam = pMaskTmpObject->pRawMaskDrawParam[expression];
                    FFL.iDrawRawMask(pDrawParam, &pCallback);

                    cl.PopDebugGroup();
                }

                // ok finally set that texture as the texture for that mask yes. TODO: SetExpression FUNCTION
                FFLDrawParam* pMaskDrawParam = FFL.GetDrawParamXluMask(&charModel); // usually returned as const
                maskTextureHandle = _textureCallbackHandler.AddTextureToMap(MaskTextures[expression]);
                pMaskDrawParam->modulateParam.pTexture2D = (void*)maskTextureHandle;
                // faceline texture param
                if (*ppFacelineTexture2D != null) // otherwise it is bound as const color so it ok
                {
                    FFLDrawParam* pFaceDrawParam = FFL.GetDrawParamOpaFaceline(&charModel);
                    faceTextureHandle = _textureCallbackHandler.AddTextureToMap(FacelineTexture);
                    pFaceDrawParam->modulateParam.pTexture2D = (void*)faceTextureHandle;
                }
            }
            cl.PopDebugGroup(); // Draw Faceline and Masks

            cl.End();
            gd.SubmitCommands(cl);

            _shaderCallbackHandler.PipelineMode = ShaderCallbackHandler.PipelineModeType.DefaultPipeline;

            // cleanup!!!
            unsafe
            {
                FFLiCharModel* pCharModel = (FFLiCharModel*)&charModel;
                void** ppFacelineTexture2D = (void**)&pCharModel->facelineRenderTexture; // HACK: FFLiRenderTexture = FFLTexture**
                if (*ppFacelineTexture2D != null)
                    FFL.iDeleteTempObjectFacelineTexture(&pCharModel->pTextureTempObject->facelineTexture, &pCharModel->charInfo, pCharModel->charModelDesc.resourceType);
                FFL.iDeleteTempObjectMaskTextures(&pCharModel->pTextureTempObject->maskTextures, pCharModel->charModelDesc.allExpressionFlag, pCharModel->charModelDesc.resourceType);
                FFL.iDeleteTextureTempObject(pCharModel);
            }

            // model matrix is fine

            // proj
            float aspect = (float)gd.SwapchainFramebuffer.Width / (float)gd.SwapchainFramebuffer.Height;
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView: 0.26179939f, // 15 degrees = 0.26179939 radians
                aspectRatio: aspect, nearPlaneDistance: 1.0f, farPlaneDistance: 1000.0f);
            // view
            _viewMatrix = Matrix4x4.CreateLookAt(
                cameraPosition: new Vector3(0.0f, 3.70f, 38.0f),
                cameraTarget: new Vector3(0.0f, 3.70f, 0.0f),
                cameraUpVector: new Vector3(0.0f, 1.0f, 0.0f)
            );

            // Rendering loop
            while (_window.Exists)
            {
                _window.PumpEvents();
                if (!_window.Exists) { break; }

//cl.Begin();

                // Start rendering to the framebuffer
                cl.SetFramebuffer(gd.SwapchainFramebuffer);

                cl.PushDebugGroup("Pre-Frame: Clear Color, Depth, Set View Uniforms");

                cl.ClearColorTarget(0, new RgbaFloat(0.2f, 0.3f, 0.3f, 1.0f));
                cl.ClearDepthStencil(1.0f); // Drawing shapes. Need this.

                _shaderCallbackHandler.SetViewUniforms(_modelMatrix, _viewMatrix, _projectionMatrix);

//_shaderCallbackHandler.CurrentFramebuffer = gd.SwapchainFramebuffer;
//cl.End();
//gd.SubmitCommands(cl);

                cl.PopDebugGroup();

                // Effectively binds pipeline, vertex buffer, resource set
                unsafe
                {
                    fixed (FFLShaderCallback* pCallback = &_shaderCallback)
                    {
                        cl.PushDebugGroup("FFLDrawOpa");
                        FFL.DrawOpaWithCallback((FFLCharModel*)Unsafe.AsPointer(ref charModel), pCallback);
                        cl.PopDebugGroup();
                        cl.PushDebugGroup("FFLDrawXlu");
                        
/*
                        FFLiCharModel* pCharModel = (FFLiCharModel*)&charModel;
                        fixed (FFLDrawParam* pDrawParam = &pCharModel->drawParam[(int)FFLiShapeType.FFLI_SHAPE_TYPE_OPA_FACELINE])
                        {
                            _shaderCallbackHandler.Draw(pDrawParam);
                        }
*/
        
                        FFL.DrawXluWithCallback((FFLCharModel*)Unsafe.AsPointer(ref charModel), pCallback);
                        cl.PopDebugGroup();
                    }
                    // NOTE: for whatever reason, calling DrawOpa/Xlu and not WithCallback...
                    // ... causes the program to crash eventually and the texture callback
                    // changes to null pointers? perhaps it is garbage collected?
                }


                // Set primary triangle pipeline
                /*
                cl.SetPipeline(pipeline);
                cl.SetVertexBuffer(0, vertexBuffer);
                cl.Draw(vertexCount: 3, instanceCount: 1, vertexStart: 0, instanceStart: 0);
                */
                
                cl.End();
                gd.SubmitCommands(cl);
                
                
                gd.SwapBuffers();
            }

            gd.WaitForIdle();

            // Dispose resources
            DeleteCharModel(charModel);
            CleanupFFL();
            /*
            vertexBuffer.Dispose();
            foreach (Shader shader in shaders)
            {
                shader.Dispose();
            }
            pipeline.Dispose();*/


            _textureCallbackHandler.DisposeTextureHandle(maskTextureHandle);

            _facelineFramebuffer?.Dispose();
            foreach (var framebuffer in _maskFramebuffers)
            {
                framebuffer?.Dispose();
            }

            cl.Dispose();
            gd.Dispose();
        }

        public void Dispose()
        {
            // Free callback GCHandles
            if (_textureCallbackHandle.IsAllocated)
            {
                _textureCallbackHandle.Free();
                _textureCallbackHandlerHandle.Free();
            }
            if (_shaderCallbackHandle.IsAllocated)
            {
                _shaderCallbackHandle.Free();
                _shaderCallbackHandlerHandle.Free();
            }

            // handled automatically if we put it
            // in the texture callback handler class? lol?
            //_textureCallbackHandler.DisposeTextureHandle();
            /*
            FacelineTexture?.Dispose();
            foreach (var texture in MaskTextures)
            {
                texture?.Dispose();
            }
            */

            // Dispose callback handlers
            _textureCallbackHandler?.Dispose();
            _shaderCallbackHandler?.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VertexPositionColor
    {
        public Vector2 Position;
        public RgbaFloat Color;

        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
    }




}
