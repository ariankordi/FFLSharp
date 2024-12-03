using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;

using FFLSharp.Interop;
using static FFLSharp.BasicTest.Program;
using System.Runtime.CompilerServices; // for InitializeFFL, CleanupFFL

namespace FFLSharp.TextureCallbackTestVeldrid
{
    class Program
    {
        private const string windowTitle = "trying to load textures from ffl into veldrid";

        private static TextureCallbackHandler _textureCallbackHandler;
        private static FFLTextureCallback _textureCallback;
        private static GCHandle _textureCallbackGCHandle;

        static void Main(string[] args)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: true,
                swapchainDepthFormat: null,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved);

            GraphicsBackend backend = GraphicsBackend.OpenGL;//VeldridStartup.GetPlatformDefaultBackend();

            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, 800, 600, WindowState.Normal, windowTitle),
                options,
                backend,
                out Sdl2Window _window,
                out GraphicsDevice gd);

            // initialize ffflllllll
            InitializeFFL();

            // initialize texture callback handler
            _textureCallbackHandler = new TextureCallbackHandler(gd);

            // Assign the delegates
            unsafe
            {
                // Create a GCHandle for the TextureCallbackHandler instance
                _textureCallbackGCHandle = GCHandle.Alloc(_textureCallbackHandler);

                // Set up the FFLTextureCallback struct
                _textureCallback.pObj = (void*)GCHandle.ToIntPtr(_textureCallbackGCHandle);

                _textureCallback.pCreateFunc = &TextureCallbackHandler.CreateTextureCallback;
                _textureCallback.pDeleteFunc = &TextureCallbackHandler.DeleteTextureCallback;

                // Pass the callback to FFL
                fixed (FFLTextureCallback* pCallback = &_textureCallback)
                {
                    FFL.SetTextureCallback(pCallback);
                }
            }

            ResourceFactory factory = gd.ResourceFactory;
            CommandList cl = factory.CreateCommandList();

            // Define vertex structure
            VertexPositionColor[] vertices =
            {
                new VertexPositionColor(new Vector2(0.0f,  0.5f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(0.5f, -0.5f), RgbaFloat.Green),
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
            Shader vs = factory.CreateShader(new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(
                @"#version 330 core
                  layout(location = 0) in vec2 Position;
                  layout(location = 1) in vec4 Color;
                  out vec4 fsin_Color;
                  void main()
                  {
                      gl_Position = vec4(Position, 0.0, 1.0);
                      fsin_Color = Color;
                  }"),
                "main"));

            Shader fs = factory.CreateShader(new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(
                @"#version 330 core
                  in vec4 fsin_Color;
                  out vec4 fsout_Color;
                  void main()
                  {
                      fsout_Color = fsin_Color;
                  }"),
                "main"));

            Shader[] shaders = { vs, fs };

            // Define vertex layout
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new[] { vertexLayout },
                    shaders: shaders),
                ResourceLayouts = Array.Empty<ResourceLayout>(), // No resources used
                Outputs = gd.SwapchainFramebuffer.OutputDescription
            };

            Pipeline pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            // Set up the shader callback
            /*
            ShaderInstance shaderInstance = new ShaderInstance(gd);
            GCHandle handle = GCHandle.Alloc(shaderInstance);
            IntPtr pObjPtr = GCHandle.ToIntPtr(handle);
            unsafe
            {
                FFLShaderCallback shaderCallback = new FFLShaderCallback
                {
                    pObj = (void*)pObjPtr,
                    pApplyAlphaTestFunc = &ShaderInstance.ApplyAlphaTestCallback,
                    pDrawFunc = &ShaderInstance.DrawCallback,
                    pSetMatrixFunc = &ShaderInstance.SetMatrixCallback
                };

                // Register the shader callback with FFL
                FFL.SetShaderCallback(&shaderCallback);
            }
            */

            FFLCharModel charModel = new FFLCharModel();

            FFLResult result = CreateCharModelFromStoreData(ref charModel, cJasmineStoreData); // FFLInitCharModelCPUStep
            Console.WriteLine($"CreateCharModelFromStoreData result: {(int)result}");
            unsafe
            {
                //FFL.InitCharModelGPUStep((FFLCharModel*)Unsafe.AsPointer(ref charModel));
                FFL.DrawOpa((FFLCharModel*)Unsafe.AsPointer(ref charModel));
            }

            // Rendering loop
            while (_window.Exists)
            {
                _window.PumpEvents();
                if (!_window.Exists) { break; }

                cl.Begin();
                cl.SetFramebuffer(gd.SwapchainFramebuffer);
                cl.ClearColorTarget(0, new RgbaFloat(0.2f, 0.3f, 0.3f, 1.0f));
                cl.SetPipeline(pipeline);
                cl.SetVertexBuffer(0, vertexBuffer);
                cl.Draw(vertexCount: 3, instanceCount: 1, vertexStart: 0, instanceStart: 0);
                cl.End();
                gd.SubmitCommands(cl);
                gd.SwapBuffers();
            }

            gd.WaitForIdle();

            // Dispose resources
            DeleteCharModel(charModel);
            CleanupFFL();

            vertexBuffer.Dispose();
            foreach (Shader shader in shaders)
            {
                shader.Dispose();
            }
            pipeline.Dispose();
            cl.Dispose();
            gd.Dispose();
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
