using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Vulkan;

namespace FFLSharp.VeldridRenderer
{
    public class CharModelResource : IDisposable, ICharModelResource
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

        public CharModelResource(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _factory = graphicsDevice.ResourceFactory;

            // Load shaders from ShaderSources class.
            LoadShaders(ShaderSources.VertexShaderDefault3DCode, ShaderSources.VertexShader2DPlaneCode, ShaderSources.FragmentShaderCode);
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
            xluPipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend; // Use alpha blending.

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
    }

    /*
    public Pipeline GetPipeline(PipelineKey key)
    {
        if (!_pipelines.TryGetValue(key, out Pipeline pipeline))
        {
            pipeline = CreatePipeline(key);
            _pipelines[key] = pipeline;
        }

        return pipeline;
    }

    private Pipeline CreatePipeline(PipelineKey key)
    {
        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = key.BlendState,
            DepthStencilState = key.DepthStencilState,
            RasterizerState = key.RasterizerState,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(
                vertexLayouts: key.VertexLayouts,
                shaders: _shaders),
            ResourceLayouts = new[] { _resourceLayout },
            Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
        };

        return _factory.CreateGraphicsPipeline(ref pipelineDescription);
    }
    */

    /*
        public struct PipelineKey : IEquatable<PipelineKey>
        {
            public BlendStateDescription BlendState;
            public DepthStencilStateDescription DepthStencilState;
            public RasterizerStateDescription RasterizerState;
            public VertexLayoutDescription[] VertexLayouts;

            public bool Equals(PipelineKey other)
            {
                // Implement equality comparison based on fields
                return BlendState.Equals(other.BlendState)
                    && DepthStencilState.Equals(other.DepthStencilState)
                    && RasterizerState.Equals(other.RasterizerState)
                    && VertexLayoutsEquals(VertexLayouts, other.VertexLayouts);
            }

            public override int GetHashCode()
            {
                // Implement hash code calculation based on fields
                int hashCode = BlendState.GetHashCode();
                hashCode = (hashCode * 397) ^ DepthStencilState.GetHashCode();
                hashCode = (hashCode * 397) ^ RasterizerState.GetHashCode();
                hashCode = (hashCode * 397) ^ VertexLayoutsHashCode(VertexLayouts);
                return hashCode;
            }

            private bool VertexLayoutsEquals(VertexLayoutDescription[] a, VertexLayoutDescription[] b)
            {
                if (a.Length != b.Length)
                    return false;

                for (int i = 0; i < a.Length; i++)
                {
                    if (!a[i].Equals(b[i]))
                        return false;
                }

                return true;
            }

            private int VertexLayoutsHashCode(VertexLayoutDescription[] layouts)
            {
                int hashCode = 0;
                foreach (var layout in layouts)
                {
                    hashCode = (hashCode * 397) ^ layout.GetHashCode();
                }
                return hashCode;
            }
        }
    */
}
