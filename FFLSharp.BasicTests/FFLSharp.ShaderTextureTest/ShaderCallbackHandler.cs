using FFLSharp.Interop;
using FFLSharp.TextureCallbackTestVeldrid;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace FFLSharp.ShaderTextureTestVeldrid
{
    public unsafe class ShaderCallbackHandler : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly CommandList _commandList;
        private readonly ResourceFactory _factory;

        // Shaders and Pipeline
        private Shader[] _shaders;
        private Shader[] _shadersRenderTexture;

        private Pipeline _pipeline;
        private Pipeline _pipelineFaceline;
        private Pipeline _pipelineMask;

        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;

        // Index and Vertex Buffers
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer[] _vertexBuffers = new DeviceBuffer[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_MAX];

        // Uniform Buffers
        private DeviceBuffer _vertexUniformBuffer;
        private DeviceBuffer _fragmentUniformBuffer;

        // Uniform data structures
        private VertexUniforms _vertexUniforms;
        private FragmentUniforms _fragmentUniforms;

        // Texture and Sampler
        private Sampler _sampler;

        // Texture Management
        private readonly TextureCallbackHandler _textureCallbackHandler;

        // GCHandle to prevent garbage collection
        private readonly GCHandle _gcHandle;

        private Texture _defaultTexture;
        private TextureView _defaultTextureView;
        private TextureView _textureView; // default single texture view

        public PipelineModeType PipelineMode; // Settable by the caller.
                                              // Indicates which pipeline to bind
                                              // TODO: MAY NOT BE THREAD SAFE???
        //public Framebuffer CurrentFramebuffer; // HACK!!!

        // TODO: look into whether or not it would still
        // be worth it to do a transparent faceline texture
        public enum PipelineModeType
        {
            DefaultPipeline,
            FacelineTexturePipeline,
            MaskTexturePipeline,
        }
        public ShaderCallbackHandler(GraphicsDevice graphicsDevice, CommandList commandList, TextureCallbackHandler textureCallbackHandler)
        {
            _graphicsDevice = graphicsDevice;
            _commandList = commandList;
            _factory = graphicsDevice.ResourceFactory;
            _textureCallbackHandler = textureCallbackHandler;

            InitializeResources();

            // Pin this instance for callbacks
            _gcHandle = GCHandle.Alloc(this);
        }

        private void InitializeResources()
        {
            LoadShaders();
            InitializeDummyBufferAndTexture();
            CreateResources();
            CreatePipeline();
            CreateSampler();
        }

        private void LoadShaders()
        {
            // Define shaders into strings
            const string vertexShaderCode = @"
            #version 450 core

            layout(set = 0, binding = 0) uniform VertexUniforms
            {
                mat4 u_mv;
                mat4 u_proj;
            } vertexUniforms;

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
/*
            layout(location = 2) in vec3 a_normal;    // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
            layout(location = 3) in vec3 a_tangent;   // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
            layout(location = 4) in vec4 a_color;     // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR
                
            layout(location = 0) out vec4 v_color;
*/
            layout(location = 1) out vec4 v_position;
            //layout(location = 2) out vec3 v_normal;
            //layout(location = 3) out vec3 v_tangent;
            layout(location = 4) out vec2 v_texCoord;

            void main()
            {
                v_position = vertexUniforms.u_mv * a_position;
                gl_Position = vertexUniforms.u_proj * v_position;

                //v_normal = a_normal;
                //v_tangent = v_tangent;
                v_texCoord = a_texCoord;
                //v_color = a_color;

            }
            ";

            // This vertex shader has no uniforms or lighting attributes
            // and is meant for drawing mask and faceline textures.
            const string vertexShaderForTextureCode = @"
            #version 450 core

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD

            layout(location = 4) out vec2 v_texCoord;

            void main()
            {
                gl_Position = vec4(a_position.xyz, 1.0);
                v_texCoord = a_texCoord;
            }
            ";


            const string fragmentShaderCode = @"
            #version 450 core

            layout(set = 0, binding = 1) uniform FragmentUniforms
            {
                int u_mode;
                vec3 u_const1;
                vec3 u_const2;
                vec3 u_const3;
            } fragmentUniforms;

            layout(set = 0, binding = 2) uniform sampler2D Texture;

            //layout(location = 0) in vec4 v_color;
            layout(location = 1) in vec4 v_position;
            /*layout(location = 2) in vec3 v_normal;
            layout(location = 3) in vec3 v_tangent;
            */layout(location = 4) in vec2 v_texCoord;

            layout(location = 0) out vec4 FragColor;

            void main()
            {
                vec4 texColor = texture(Texture, v_texCoord); // Sample from the texture

                if (fragmentUniforms.u_mode == 0)
                    FragColor = vec4(fragmentUniforms.u_const1, 1.0);
                else if (fragmentUniforms.u_mode == 1)
                    FragColor = texColor;
                else if (fragmentUniforms.u_mode == 2)
                    FragColor = vec4(
                        fragmentUniforms.u_const1 * texColor.r +
                        fragmentUniforms.u_const2 * texColor.g +
                        fragmentUniforms.u_const3 * texColor.b,
                        texColor.a
                    );
                else if (fragmentUniforms.u_mode == 3)
                    FragColor = vec4(
                        fragmentUniforms.u_const1 * texColor.r,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 4)
                    FragColor = vec4(
                        fragmentUniforms.u_const1 * texColor.g,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 5)
                    FragColor = vec4(
                        fragmentUniforms.u_const1 * texColor.r,
                        1.0
                    );
            }
            ";

            // Create shaders
            _shaders = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(vertexShaderCode),
                "main"),
                new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(fragmentShaderCode),
                "main")
            );

            // Shaders for render texture pipeline
            _shadersRenderTexture = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex,
                    Encoding.UTF8.GetBytes(vertexShaderForTextureCode),
                "main"),
                new ShaderDescription(ShaderStages.Fragment,
                    Encoding.UTF8.GetBytes(fragmentShaderCode),
                "main")
            );
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexUniforms
        {
            public Matrix4x4 u_mv;
            public Matrix4x4 u_proj;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FragmentUniforms
        {
            public int u_mode;
            private readonly float padding1;
            private readonly float padding2;
            private readonly float padding3;
            public Vector3 u_const1;
            private readonly float padding4;
            public Vector3 u_const2;
            private readonly float padding5;
            public Vector3 u_const3;
            private readonly float padding6;
        }
        public void SetViewUniforms(Matrix4x4 model, Matrix4x4 view, Matrix4x4 proj)
        {
            _vertexUniforms.u_mv = model * view;
            _vertexUniforms.u_proj = proj;
            //_vertexUniforms.u_mvp = Matrix4x4.Identity;//CreateScale(0.01f, 0.01f, 0.01f);

            // Update the vertex uniform buffer
            _graphicsDevice.UpdateBuffer(_vertexUniformBuffer, 0, ref _vertexUniforms);
        }

        private void SetFragmentUniforms(FFLModulateParam pParam)
        {
            _fragmentUniforms.u_mode = (int)pParam.mode;
            _fragmentUniforms.u_const1 = pParam.pColorR != null
                ? new Vector3(pParam.pColorR->r, pParam.pColorR->g, pParam.pColorR->b)
                : Vector3.Zero;
            _fragmentUniforms.u_const2 = pParam.pColorG != null
                ? new Vector3(pParam.pColorG->r, pParam.pColorG->g, pParam.pColorG->b)
                : Vector3.Zero;
            _fragmentUniforms.u_const3 = pParam.pColorB != null
                ? new Vector3(pParam.pColorB->r, pParam.pColorB->g, pParam.pColorB->b)
                : Vector3.Zero;

            // Update the fragment uniform buffer
            _commandList.UpdateBuffer(_fragmentUniformBuffer, 0, ref _fragmentUniforms);
        }

        private void CreateResources()
        {
            _vertexUniformBuffer = _factory.CreateBuffer(new BufferDescription(
                (uint)Marshal.SizeOf<VertexUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _fragmentUniformBuffer = _factory.CreateBuffer(new BufferDescription(
                (uint)Marshal.SizeOf<FragmentUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _resourceLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("VertexUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("FragmentUniforms", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));
        }

        private void CreatePipeline()
        {
            // Define vertex layouts for each attribute buffer
            // All semantics are TextureCoordinate because uhhhhhh
            // thaaaat's what Veldrid.SPIRV emits for some reason.
            var vertexLayouts = new[]
            {
                // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
                new VertexLayoutDescription(
                    stride: sizeof(float) * 4, // stride = 16
                    new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
                // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
                new VertexLayoutDescription(
                    stride: sizeof(float) * 2,
                    new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)),
                /*
                // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
                new VertexLayoutDescription(
                    stride: sizeof(int), // 32 bit GL_INT_2_10_10_10_REV
                    new VertexElementDescription("a_normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)),
                // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
                new VertexLayoutDescription(
                    stride: sizeof(byte) * 4, // 8_8_8_8 Snorm
                    new VertexElementDescription("a_tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)),
                // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR
                new VertexLayoutDescription(
                    stride: sizeof(byte) * 4, // 4 bytes
                    new VertexElementDescription("a_color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)),
                */
            };

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual),
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back, // TODO: but glass cull mode is none!!!
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: vertexLayouts,
                    shaders: _shaders),
                ResourceLayouts = new ResourceLayout[] { _resourceLayout },
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            };

            // Get the swapchain's color format - assuming the render texture will match it.
            PixelFormat swapchainFormat = _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            // Pretty much the same as the swapchain output description but with no depth buffer at all.
            OutputDescription renderTextureOutputDescription = new OutputDescription(null, new OutputAttachmentDescription(swapchainFormat));

            // Pipeline for render textures.

            GraphicsPipelineDescription pipelineFacelineDescription = new GraphicsPipelineDescription
            {
                BlendState = new BlendStateDescription
                {
                    BlendFactor = RgbaFloat.Clear,
                    AttachmentStates = new[]
                    {
                        new BlendAttachmentDescription
                        {
                            BlendEnabled = true,
                            SourceColorFactor = BlendFactor.SourceAlpha,
                            DestinationColorFactor = BlendFactor.InverseSourceAlpha,
                            ColorFunction = BlendFunction.Add,
                            SourceAlphaFactor = BlendFactor.One,
                            DestinationAlphaFactor = BlendFactor.One,
                            AlphaFunction = BlendFunction.Add
                        }
                    }
                },
                DepthStencilState = DepthStencilStateDescription.Disabled, // No depth
                RasterizerState = RasterizerStateDescription.CullNone,     // No culling
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,       // Triangle strip
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: vertexLayouts,
                    shaders: _shadersRenderTexture),                       // This vertex shader has no uniforms
                ResourceLayouts = new ResourceLayout[] { _resourceLayout },
                Outputs = renderTextureOutputDescription,
            };


            GraphicsPipelineDescription pipelineMaskDescription = new GraphicsPipelineDescription
            {
                BlendState = new BlendStateDescription
                {
                    BlendFactor = RgbaFloat.Clear,
                    AttachmentStates = new[]
                    {
                        new BlendAttachmentDescription
                        {
                            BlendEnabled = true,
                            SourceColorFactor = BlendFactor.InverseDestinationAlpha,
                            DestinationColorFactor = BlendFactor.DestinationAlpha,
                            ColorFunction = BlendFunction.Add,
                            SourceAlphaFactor = BlendFactor.SourceAlpha,
                            DestinationAlphaFactor = BlendFactor.DestinationAlpha,
                            AlphaFunction = BlendFunction.Add
                        }
                    }
                },
                DepthStencilState = DepthStencilStateDescription.Disabled, // No depth
                RasterizerState = RasterizerStateDescription.CullNone,     // No culling
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,       // Triangle strip
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: vertexLayouts,
                    shaders: _shadersRenderTexture),                       // This vertex shader has no uniforms
                ResourceLayouts = new ResourceLayout[] { _resourceLayout },
                Outputs = renderTextureOutputDescription,
            };


            _pipeline = _factory.CreateGraphicsPipeline(ref pipelineDescription);
            _pipelineFaceline = _factory.CreateGraphicsPipeline(ref pipelineFacelineDescription);
            _pipelineMask = _factory.CreateGraphicsPipeline(ref pipelineMaskDescription);
        }

        private void CreateSampler()
        {
            _sampler = _factory.CreateSampler(new SamplerDescription
            {
                AddressModeU = SamplerAddressMode.Mirror,
                AddressModeV = SamplerAddressMode.Mirror,
                AddressModeW = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                MinimumLod = 0,
                MaximumLod = 16,
                MaximumAnisotropy = 0,
            });
        }

        // Implement the static methods for shader callbacks
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void DrawCallback(void* pObj, FFLDrawParam* pDrawParam)
        {
            var handler = (ShaderCallbackHandler)GCHandle.FromIntPtr((IntPtr)pObj).Target;
            handler.Draw(pDrawParam);
        }

        // Instance methods to handle shader operations
        public void Draw(FFLDrawParam* pDrawParam) // Public because you can call this yourself on FFLiCharModel draw params.
        {
/*
            _commandList.Begin();
            _commandList.SetFramebuffer(CurrentFramebuffer);
*/          

            // Verify whether or not this even has indices.
            // FFLiCanDrawShape does this very check
            if (pDrawParam->primitiveParam.indexCount == 0
            )//|| pDrawParam->modulateParam.type == FFLModulateType.FFL_MODULATE_TYPE_SHAPE_HAIR)
            {
                // This shape cannot be drawn (probably has no attributes)
                // Will be skipped
                return;
            }
        
            // Map FFLDrawParam to Veldrid draw calls
            // This includes setting up vertex buffers, index buffers, uniforms, and issuing draw commands

            // Set cull mode
            //SetCullMode(pDrawParam->cullMode);

            // Bind the index buffer
            BindIndexBuffer(&pDrawParam->primitiveParam);

            // Bind vertex buffers for each attribute
            BindAttributeBuffers(pDrawParam);

            // Update uniforms
            // Assume that the projection and model-view matrices are already set

            // Set fragment uniforms based on modulate params
            SetFragmentUniforms(pDrawParam->modulateParam);

            // Bind resource sets (textures and samplers)
            UpdateResourceSet(pDrawParam);

            // Set the pipeline based on the mode
            _commandList.SetPipeline(PipelineMode switch
            {
                PipelineModeType.DefaultPipeline => _pipeline,
                PipelineModeType.FacelineTexturePipeline => _pipelineFaceline,
                PipelineModeType.MaskTexturePipeline => _pipelineMask,
                _ => throw new NotSupportedException($"Unsupported pipeline mode type: {PipelineMode}"),
            });
            // Bind the resource set
            _commandList.SetGraphicsResourceSet(0, _resourceSet);

            // Issue the draw call
            _commandList.DrawIndexed(
                indexCount: pDrawParam->primitiveParam.indexCount,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
/*
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
*/
        }

        private void InitializeDummyBufferAndTexture()
        {
            // Create a 1x1 transparent texture
            _defaultTexture = _factory.CreateTexture(TextureDescription.Texture2D(
                width: 1,
                height: 1,
                mipLevels: 1,
                arrayLayers: 1,
                format: PixelFormat.R8_G8_B8_A8_UNorm,
                usage: TextureUsage.Sampled));

            // Transparent pixel data (RGBA = 0, 0, 0, 0)
            byte[] transparentPixel = new byte[] { 0, 0, 0, 0 };
            fixed (byte* pPixelData = transparentPixel)
            {
                _graphicsDevice.UpdateTexture(
                    _defaultTexture,
                    new IntPtr(pPixelData),
                    sizeInBytes: 4, // 1 RGBA pixel
                    x: 0,
                    y: 0,
                    z: 0,
                    width: 1,
                    height: 1,
                    depth: 1,
                    mipLevel: 0,
                    arrayLayer: 0);
            }

            _defaultTextureView = _factory.CreateTextureView(_defaultTexture);
        }

        private void UpdateResourceSet(FFLDrawParam* pDrawParam)
        {
            if (pDrawParam != null && pDrawParam->modulateParam.pTexture2D != null)
            {
                UIntPtr textureHandle = (UIntPtr)pDrawParam->modulateParam.pTexture2D;

                if (_textureCallbackHandler.TextureMap.TryGetValue(textureHandle, out Texture? texture))
                {
                    // Dispose the old resource set and texture view
                    _resourceSet?.Dispose();
                    _textureView?.Dispose();

                    _textureView = _factory.CreateTextureView(texture);

                    // Create a new resource set with the current texture
                    _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                        _resourceLayout,
                        _vertexUniformBuffer,
                        _fragmentUniformBuffer,
                        _textureView,
                        _sampler));
                    return;
                }
                else
                {
                    Console.WriteLine($"Texture not found for handle: {textureHandle}. Skipping texture binding.");
                }
            }
            // No texture is provided, don't bind any texture
            UpdateResourceSetWithoutTexture();
        }
        private void UpdateResourceSetWithoutTexture()
        {
            // Dispose the old resource set and texture view
            _resourceSet?.Dispose();
            _textureView?.Dispose();

            // Create a resource set without binding a texture
            _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                _resourceLayout,
                _vertexUniformBuffer,
                _fragmentUniformBuffer,
                _defaultTextureView, // Ensure _defaultTexture is a TextureView
                _sampler));
        }

        private void BindIndexBuffer(FFLPrimitiveParam* pPrimitiveParam)
        {
            // Bind Index Buffer
            //if (pPrimitiveParam->pIndexBuffer != null)
            //{
                uint indexCount = pPrimitiveParam->indexCount;
                ushort* indices = (ushort*)pPrimitiveParam->pIndexBuffer;

                // Dispose the old index buffer if it exists
                _indexBuffer?.Dispose();

                _indexBuffer = _factory.CreateBuffer(new BufferDescription(
                    indexCount * sizeof(ushort),
                    BufferUsage.IndexBuffer | BufferUsage.Dynamic));
                IntPtr indexBufferPtr = (IntPtr)pPrimitiveParam->pIndexBuffer;

                _commandList.UpdateBuffer(_indexBuffer, 0, indexBufferPtr, (indexCount * sizeof(ushort)));
                _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            //}
        }

        private void BindAttributeBuffers(FFLDrawParam* pDrawParam)
        {
            // Iterate over each attribute buffer
            // NOTE: as a test this is stoppping at normal
            for (FFLAttributeBufferType type = 0; type < FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL; type++) // MAX; type++)
            {
                FFLAttributeBuffer attrBuffer = pDrawParam->attributeBufferParam.attributeBuffers[(int)type];

                if (attrBuffer.ptr == null || attrBuffer.stride < 0) // (ptr != NULL && location != -1 && buffer->stride > 0)
                {
                    // die
                    Console.WriteLine($"Attribute {type} is missing for: {pDrawParam->modulateParam.type}");
                    continue;
                }
                
                _vertexBuffers[(int)type]?.Dispose();

                // Create a Veldrid buffer using the attribute buffer data directly
                _vertexBuffers[(int)type] = _factory.CreateBuffer(new BufferDescription(
                    sizeInBytes: attrBuffer.size,
                    usage: BufferUsage.VertexBuffer));

                // Update the buffer with the data directly from the pointer
                _commandList.UpdateBuffer(_vertexBuffers[(int)type], 0, (IntPtr)attrBuffer.ptr, attrBuffer.size);

                // Bind the buffer to the corresponding slot (type index as slot)
                _commandList.SetVertexBuffer((uint)type, _vertexBuffers[(int)type]);
            }
        }

        // Dispose pattern
        public void Dispose()
        {
            foreach (var shader in _shaders)
            {
                shader.Dispose();
            }
            foreach (var shader in _shadersRenderTexture)
            {
                shader.Dispose();
            }

            _pipeline.Dispose();
            _pipelineFaceline.Dispose();
            _pipelineMask.Dispose();
            _vertexUniformBuffer.Dispose();
            _fragmentUniformBuffer.Dispose();
            _sampler.Dispose();
            _resourceLayout.Dispose();
            _resourceSet?.Dispose();
            _defaultTexture.Dispose();
            _textureView?.Dispose();
            _defaultTextureView?.Dispose();
            foreach (var vertexBuffer in _vertexBuffers)
            {
                vertexBuffer?.Dispose();
            }
            _indexBuffer?.Dispose();

            _gcHandle.Free();
        }
    }
}
