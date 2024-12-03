using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Vulkan;

namespace FFLSharp.Veldrid
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

            LoadShaders();
            ResourceLayout = CreateResourceLayout();
            CreatePipelines();
            CreateDefaultTextureAndSampler();
        }

        private void LoadShaders()
        {
            #region Shader Code

            // Defining shader code into strings for now.
            // Vertex shader corresponding to PipelineDefault3DShape
            const string vertexShaderDefault3DCode = @"#version 310 es
            precision highp float;

            layout(set = 0, binding = 0) uniform VertexUniforms
            {
                mat4 u_mv;
                mat4 u_proj;
            } vertexUniforms;
            // Binding 0 = VertexUniforms / ResourceKind.UniformBuffer

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD

            layout(location = 2) in vec3 a_normal;    // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
//          layout(location = 3) in vec3 a_tangent;   // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
//          layout(location = 4) in vec4 a_color;     // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR


            layout(location = 0) out vec2 v_texCoord;
            layout(location = 1) out vec4 v_position;

            layout(location = 2) out vec3 v_normal;
            layout(location = 3) out vec3 v_tangent;
            layout(location = 4) out vec4 v_color;


            void main()
            {
                vec4 position = vec4(a_position.xyz, 1.0);
                vec4 transformed = vertexUniforms.u_mv * position;
                gl_Position = vertexUniforms.u_proj * transformed;

                v_position = transformed;
                v_normal = a_normal;
                //v_tangent = v_tangent;
                v_texCoord = a_texCoord;
                //v_color = a_color;

                //v_normal = vec3(0.0, 0.0, 0.0);
                v_tangent = vec3(0.0, 0.0, 0.0);
                v_color = vec4(0.0, 0.0, 0.0, 0.0);
            }
            ";

            // This shader has no uniforms, lighting attributes
            // and is meant for the 2D planes.
            const string vertexShader2DPlaneCode = @"#version 310 es
            precision highp float;

            layout(set = 0, binding = 0) uniform VertexUniforms
            {
                mat4 u_mv;
                mat4 u_proj;
            } vertexUniforms;
            // Binding 0 = VertexUniforms / ResourceKind.UniformBuffer

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD

            layout(location = 0) out vec2 v_texCoord;
            layout(location = 1) out vec4 v_position;

            layout(location = 2) out vec3 v_normal;
            layout(location = 3) out vec3 v_tangent;
            layout(location = 4) out vec4 v_color;

            void main()
            {
                gl_Position = vec4(a_position.xyz, 1.0);
                v_texCoord = a_texCoord;

                v_position = vertexUniforms.u_proj * a_position;
                v_normal = vec3(0.0, 0.0, 0.0);
                v_tangent = vec3(0.0, 0.0, 0.0);
                v_color = vec4(0.0, 0.0, 0.0, 0.0);
            }
            ";

            // This fragment shader should be being used for everything.
            const string fragmentShaderCode = @"#version 310 es
            precision mediump float;

            layout(set = 0, binding = 1) uniform FragmentUniforms
            {
                int u_mode;
                vec4 u_const1;
                vec4 u_const2;
                vec4 u_const3;
            } fragmentUniforms;
            // Binding 1 = FragmentUniforms / ResourceKind.UniformBuffer

            // TODO: May not be one-to-one compatible with other environments (need if VELDRID preprocessor?)
            // See: https://github.com/SupinePandora43/UltralightNet/blob/95a060fc226024a81cd9ba058d691628e1055489/gpu/shaders/shader_fill.frag#L188

            layout(set = 0, binding = 2) uniform mediump texture2D Texture;
            // Binding 2 = ResourceKind.TextureReadOnly
            layout(set = 0, binding = 3) uniform mediump sampler Sampler;
            // Binding 3 = ResourceKind.Sampler

            layout(location = 0) in vec2 v_texCoord;
            layout(location = 1) in vec4 v_position;

            layout(location = 2) in vec3 v_normal;
            layout(location = 3) in vec3 v_tangent;
            layout(location = 4) in vec4 v_color;

            layout(location = 0) out vec4 FragColor;

            void main()
            {
            //FragColor = vec4(v_texCoord.xy, 0.0, 1.0); return;
                vec4 texColor = texture(sampler2D(Texture, Sampler), v_texCoord); // Sample from the texture

                if (fragmentUniforms.u_mode == 0)
                    FragColor = fragmentUniforms.u_const1;
                else if (fragmentUniforms.u_mode == 1)
                    FragColor = texColor;
                else if (fragmentUniforms.u_mode == 2)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r +
                        fragmentUniforms.u_const2.rgb * texColor.g +
                        fragmentUniforms.u_const3.rgb * texColor.b,
                        texColor.a
                    );
                else if (fragmentUniforms.u_mode == 3)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 4)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.g,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 5)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r,
                        1.0
                    );
            }
            ";

            #endregion

            // Compile default shaders
            _shaders = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexShaderDefault3DCode), "main"),
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
            OutputDescription noDepthOutputDescription = new(depthAttachment: null,
                                        colorAttachments: new OutputAttachmentDescription(SwapchainTexFormat.Value));

            #region Faceline pipeline

            // Create pipeline for rendering 2D faceline texture.
            GraphicsPipelineDescription facelinePipelineDescription = new()
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
                            SourceAlphaFactor = BlendFactor.One,
                            DestinationAlphaFactor = BlendFactor.One,
                            AlphaFunction = BlendFunction.Maximum
                            // FFL-Testing/AFL source alpha
                            //SourceAlphaFactor = BlendFactor.SourceAlpha,
                            //DestinationAlphaFactor = BlendFactor.DestinationAlpha,
                            //AlphaFunction = BlendFunction.Add
                        }
                    }
            };

            #endregion

            #endregion // Create pipelines for 2D planes

            #region Create pipelines for 3D shapes

            // DrawOpa stage pipelines

            GraphicsPipelineDescription pipelineDescription = new()
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