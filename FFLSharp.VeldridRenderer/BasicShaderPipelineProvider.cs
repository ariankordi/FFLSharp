using FFLSharp.Interop;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Vulkan;

namespace FFLSharp.VeldridRenderer
{
    public class BasicShaderPipelineProvider : IDisposable, IPipelineProvider, IUniformProvider
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _factory;

        // Shaders used in the pipelines.
        private Shader[] _shaders;   // For most shapes.
        private Shader[] _shaders2D; // For faceline/mask texture 2D planes.


        // Pipelines for faceline/mask texture (2D plane) drawing:

        /// <summary>
        /// Meant for 2D planes, only supports position/texcoord, no culling or depth, triangle strip.
        /// Uses blending for faceline texture.
        /// </summary>
        public Pipeline PipelineFaceline2DPlane { get; private set; }
        /// <summary>
        /// Meant for 2D planes, only supports position/texcoord, no culling or depth, triangle strip.
        /// Uses blending for mask texture.
        /// </summary>
        public Pipeline PipelineMask2DPlane { get; private set; }

        // Pipelines for DrawOpa stage:

        /// <summary>
        /// Meant for all DrawOpa shapes other than hair.
        /// Position/texcoord/normal. No blending.
        /// </summary>
        public Pipeline PipelineShapeOpa { get; private set; }
        /// <summary>
        /// Special pipeline for hair shape.
        /// Position/normal/tangent/color.
        /// NOTE: May not actually be needed if we:
        /// - bind position as texcoord so that its not missing anymore
        /// - on other shapes, bind nonsense (normal?) as tangent
        /// - and for color, need fragment uniform(s) for color uniform enable and use
        /// </summary>
        //private Pipeline PipelineShapeOpaHair;

        // Pipelines for DrawXlu stage:

        /// <summary>
        /// Pipeline for DrawXlu shapes - mask, noseline, glasses.
        /// Features alpha blending and no depth writing.
        /// </summary>
        public Pipeline PipelineShapeXlu { get; private set; }
        /// <summary>
        /// Pipeline specifically for glasses.
        /// Same as DrawXlu but with cull mode set to none.
        /// </summary>
        public Pipeline PipelineShapeXluGlass { get; private set; }
        //private Pipeline PipelineHairFlipShape; // Not needed after flipping indices

        // Pipelines keyed by configuration
        //private readonly Dictionary<PipelineKey, Pipeline> _pipelines = new Dictionary<PipelineKey, Pipeline>();

        // Resource layout for uniforms, textures, and samplers.
        public ResourceLayout ResourceLayout { get; private set; }

        // Pixel format for swapchain, set only after pipelines are made.
        public PixelFormat? SwapchainTexFormat { get; private set; }

        // Create default texture and TextureView for public use
        public Texture DefaultTexture { get; private set; } // For shapes without textures.

        // Used for OpaFaceline, OpaCap, XluGlass:
        public Sampler SamplerMirror { get; private set; } // Mirored repeat (should be all NPOT textures)
        public Sampler Sampler { get; private set; } // For all other shapes/textures.

        public BasicShaderPipelineProvider(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _factory = graphicsDevice.ResourceFactory;

            // Load shaders from ShaderSources class.
            LoadShaders(BasicShaderSources.VertexShaderDefault3DCode, BasicShaderSources.VertexShader2DPlaneCode, BasicShaderSources.FragmentShaderCode);
            ResourceLayout = CreateResourceLayout();
            CreatePipelines();
            CreateDefaultTextureAndSampler();
        }

        private void LoadShaders(string vertexShaderShapeOpaCode, string vertexShader2DPlaneCode, string fragmentShaderCode)
        {
            // Compile default shaders
            _shaders = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexShaderShapeOpaCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentShaderCode), "main"));
            // Compile 2D plane shaders
            _shaders2D = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexShader2DPlaneCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentShaderCode), "main"));
        }

        private ResourceLayout CreateResourceLayout()
        {
            return _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("VertexUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("FragmentUniforms", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        }

        private void CreatePipelines()
        {
            #region Create pipelines for 2D planes

            // Get the swapchain's color format - assuming the render texture will match it.
            SwapchainTexFormat = _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            Debug.Assert(SwapchainTexFormat != null);
            // Pretty much the same as the swapchain output description but with no depth buffer at all.
            OutputDescription noDepthOutputDescription = new OutputDescription(depthAttachment: null,
                                        colorAttachments: new OutputAttachmentDescription(SwapchainTexFormat.Value));

            #region Faceline pipeline

            // Create pipeline for rendering 2D faceline texture.
            GraphicsPipelineDescription facelinePipelineDescription = new GraphicsPipelineDescription()
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
                    vertexLayouts: VertexLayouts.VertexLayoutsPosTexSeparate,//VertexLayoutsPosTexOnly,
                    shaders: _shaders2D),                                  // This vertex shader has no uniforms
                ResourceLayouts = new ResourceLayout[] { ResourceLayout },
                Outputs = noDepthOutputDescription,
            };

            #endregion

            #region Mask pipeline

            // Create faceline for 2D mask textures.
            // Copy this from the faceline pipeline, only blend state needs to change.
            GraphicsPipelineDescription maskPipelineDescription = facelinePipelineDescription;
            // Mask blending
            maskPipelineDescription.BlendState = new BlendStateDescription
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
                            // nn::mii source alpha
                            //SourceAlphaFactor = BlendFactor.One,
                            //DestinationAlphaFactor = BlendFactor.One,
                            //AlphaFunction = BlendFunction.Maximum
                            // FFL-Testing/AFL source alpha
                            SourceAlphaFactor = BlendFactor.SourceAlpha,
                            DestinationAlphaFactor = BlendFactor.DestinationAlpha,
                            AlphaFunction = BlendFunction.Add
                        }
                    }
            };

            #endregion

            #endregion // Create pipelines for 2D planes

            #region Create pipelines for 3D shapes

            // DrawOpa stage pipelines

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription()
            {
                // Disable blending for DrawOpa shapes.
                BlendState = BlendStateDescription.SingleDisabled,
                DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.Less), // LEQUAL in FFL? I think?
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back, // Every 3D shape has backface culling except glass
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: VertexLayouts.VertexLayoutsShapeDefaultSeparate,//VertexLayoutsShapeDefault,
                    shaders: _shaders),
                ResourceLayouts = new ResourceLayout[] { ResourceLayout },
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            };

            // Hair has more attributes
            /*
            GraphicsPipelineDescription hairPipelineDescription = pipelineDescription;
            hairPipelineDescription.ShaderSet = new ShaderSetDescription(
                    vertexLayouts: VertexLayouts.VertexLayoutsShapeHairSeparate,
                    shaders: _shaders);
            */

            // DrawXlu stage pipelines
            GraphicsPipelineDescription xluPipelineDescription = pipelineDescription;
            xluPipelineDescription.DepthStencilState.DepthWriteEnabled = false; // DrawXlu has no depth writing
            xluPipelineDescription.BlendState = new BlendStateDescription
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
                            SourceAlphaFactor = BlendFactor.SourceAlpha,
                            DestinationAlphaFactor = BlendFactor.One,
                            AlphaFunction = BlendFunction.Add
                        }
                    }
            };

                //BlendStateDescription.SingleAlphaBlend; // Use alpha blending.

            /*
    render_state.setBlendEnable(true);
    render_state.setBlendEquation(rio::Graphics::BLEND_FUNC_ADD);
    render_state.setBlendFactorSrcRGB(rio::Graphics::BLEND_MODE_SRC_ALPHA);
    render_state.setBlendFactorDstRGB(rio::Graphics::BLEND_MODE_ONE_MINUS_SRC_ALPHA);
    render_state.setBlendConstantColor({ 0.0f, 0.0f, 0.0f, 0.0f });


    // settings for AFL and also FFL in cemu (closer)
    render_state.setBlendEquationAlpha(rio::Graphics::BLEND_FUNC_ADD);
    render_state.setBlendFactorSrcAlpha(rio::Graphics::BLEND_MODE_SRC_ALPHA);
    render_state.setBlendFactorDstAlpha(rio::Graphics::BLEND_MODE_ONE);
            */

            // Glasses need culling set to none
            GraphicsPipelineDescription glassPipelineDescription = xluPipelineDescription; // Copy from DrawXlu desc.
            glassPipelineDescription.RasterizerState.CullMode = FaceCullMode.None;

            #endregion

            // Create all pipeline objects
            PipelineFaceline2DPlane = _factory.CreateGraphicsPipeline(ref facelinePipelineDescription);
            PipelineMask2DPlane = _factory.CreateGraphicsPipeline(ref maskPipelineDescription);

            PipelineShapeOpa = _factory.CreateGraphicsPipeline(ref pipelineDescription);
            //PipelineShapeOpaHair = _factory.CreateGraphicsPipeline(ref hairPipelineDescription);

            PipelineShapeXlu = _factory.CreateGraphicsPipeline(ref xluPipelineDescription);
            PipelineShapeXluGlass = _factory.CreateGraphicsPipeline(ref glassPipelineDescription);
        }

        /// <summary>
        /// Creates default blank texture, as well as creating the sampler.
        /// </summary>
        private unsafe void CreateDefaultTextureAndSampler()
        {
            #region Make default texture and texture view
            // Use default swapchain pixel format
            Debug.Assert(SwapchainTexFormat != null);
            // Create a 8x8 transparent texture
            DefaultTexture = _factory.CreateTexture(TextureDescription.Texture2D(
                width: 8,
                height: 8,
                mipLevels: 1,
                arrayLayers: 1,
                format: SwapchainTexFormat.Value,
                usage: TextureUsage.Sampled));

            // Create an 8x8 RGBA byte array filled with zeros
            const uint size = 8 * 8 * 4; // 8x8 texture with 4 bytes per pixel (RGBA)
            byte[] blankData = new byte[size];
            fixed (byte* pPixelData = blankData)
            {
                _graphicsDevice.UpdateTexture(
                    DefaultTexture,
                    new IntPtr(pPixelData),
                    sizeInBytes: size,
                    0, 0, 0,           // X, Y, Z offset (top-left corner)
                    8, 8, 1,           // Width, height, depth of the data
                    0, 0);             // Mip level and array layer
            }

            //DefaultTextureView = _factory.CreateTextureView(_defaultTexture);
            #endregion

            #region Create samplers
            // Create samplers
            SamplerMirror = _factory.CreateSampler(new SamplerDescription
            {
                AddressModeU = SamplerAddressMode.Mirror, // Mirror textures
                AddressModeV = SamplerAddressMode.Mirror,
                AddressModeW = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                MinimumLod = 0,
                MaximumLod = 0,
                MaximumAnisotropy = 0,
            });
            Sampler = _factory.CreateSampler(SamplerDescription.Linear);
            #endregion
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // Dispose all pipelines.
            PipelineFaceline2DPlane.Dispose();
            PipelineMask2DPlane.Dispose();

            PipelineShapeOpa.Dispose();
            //PipelineShapeOpaHair.Dispose();

            PipelineShapeXlu.Dispose();
            PipelineShapeXluGlass.Dispose();

            foreach (var shader in _shaders)
            {
                shader.Dispose();
            }

            ResourceLayout.Dispose();
            Sampler.Dispose();
            DefaultTexture.Dispose();
        }

        /// <summary>
        /// Vertex uniforms used for 3D shaders.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexUniforms
        {
            public Matrix4x4 ModelView;
            public Matrix4x4 Projection;
        }

        public DeviceBuffer CreateVertexUniformBuffer()
        {
            return _factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<VertexUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        /// <summary>
        /// Fragment uniforms used for the 2D and 3D shader.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FragmentUniforms
        {
            public int ModulateMode;
            // Padding for alignment.
            private readonly int _padding1;
            private readonly int _padding2;
            private readonly int _padding3;
            // Directly casted from FFLColor.
            public RgbaFloat ColorR;
            public RgbaFloat ColorG;
            public RgbaFloat ColorB;
        }

        public DeviceBuffer CreateFragmentUniformBuffer()
        {
            return _factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<FragmentUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        private void UpdateFragmentUniforms(DeviceBuffer uniformBuffer, FFLModulateParam modulateParam)
        {
            FragmentUniforms fragmentUniforms = new FragmentUniforms();
            fragmentUniforms.ModulateMode = (int)modulateParam.mode;
            // Set constant colors if they exist.
            unsafe // Dereferencing color pointers.
            {
                // Directly cast FFLColor to System.Numerics.Vector4.
                fragmentUniforms.ColorR = modulateParam.pColorR != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorR)
                    : RgbaFloat.Clear;
                fragmentUniforms.ColorG = modulateParam.pColorG != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorG)
                    : RgbaFloat.Clear;
                fragmentUniforms.ColorB = modulateParam.pColorB != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorB)
                    : RgbaFloat.Clear;
            }
            // Update fragment uniform buffer.
            _graphicsDevice.UpdateBuffer(uniformBuffer, 0, ref fragmentUniforms);
        }

        public void UpdateFragmentUniformBuffer(DeviceBuffer uniformBuffer, FFLModulateParam modulateParam)
        {
            throw new NotImplementedException();
        }
    }

}
