using FFLSharp.Interop;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics; // for assert
using Veldrid;
using System.Reflection;

namespace FFLSharp.VeldridRenderer
{
    public class DrawParamGpuHandler : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _factory;
        private readonly ICharModelResource _resourceManager;
        private readonly TextureManager _textureManager;

        private DeviceBuffer _indexBuffer;
        private uint _indexCount; // Set in UpdateIndexBuffer(), used in Draw().
        // Only one vertex buffer for this shape, for now.
        //private DeviceBuffer _vertexBuffer; // NOTE: used by UpdateVertexBufferSingle, discarded.
        private readonly DeviceBuffer[] _vertexBuffers =
            new DeviceBuffer[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_MAX];

        // Uniforms and uniform buffers
        private VertexUniforms _vertexUniforms;
        private readonly DeviceBuffer _vertexUniformBuffer; // ^^ Unused for 2D planes

        private FragmentUniforms _fragmentUniforms;
        private readonly DeviceBuffer _fragmentUniformBuffer;

        private ResourceSet _resourceSet;

        // ModulateParam in the initial DrawParam, where the pTexture2D pointer is stored.
        private unsafe FFLModulateParam* _pModulateParam = null; // Will clear pTexture2D on disposal.
        private UIntPtr _textureHandle = UIntPtr.Zero; // Bound texture handle.

        private TextureView? _textureView; // Currently bound TextureView.
        // If this is set, it will override the texture from the DrawParam.
        public Texture? _overrideTexture; // < Intended to be set for faceline/mask texture.

        // Current pipeline for this shape.
        private Pipeline _pipeline;

        // Store modulate type of this DrawParam.
        //public FFLModulateType ModulateType;

        /// <summary>
        /// Manages resources for a single DrawParam.
        /// </summary>
        /// <param name="graphicsDevice">Veldrid GraphicsDevice.</param>
        /// <param name="resourceManager">ResourceManager instance containing pipelines, resource layout, shaders.</param>
        /// <param name="textureManager">TextureManager containing textures referenced by DrawParams.</param>
        /// <param name="drawParam">FFLDrawParam instance containing current shape to render.</param>
        /// <param name="overrideTexture">Optional texture that will override the DrawParam texture if set. Intended for mask and faceline.</param>
        public DrawParamGpuHandler(GraphicsDevice graphicsDevice, ICharModelResource resourceManager,
            TextureManager textureManager, ref FFLDrawParam drawParam, Texture? overrideTexture = null)
        {
            _graphicsDevice = graphicsDevice;
            _factory = graphicsDevice.ResourceFactory;
            _resourceManager = resourceManager;
            _textureManager = textureManager;
            _overrideTexture = overrideTexture;

            // Note: Index buffer must not be blank, the caller should ensure this.
            UpdateIndexBuffer(drawParam.primitiveParam);
            SetPipeline(drawParam.modulateParam.type); // Attribute binding depends on vertexLayout being set correctly.
            //UpdateVertexBufferSingle(drawParam.attributeBufferParam); // Depends on ^^
            UpdateVertexBuffers(drawParam.attributeBufferParam);

            // Create uniform buffers.
            _vertexUniformBuffer = _factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<VertexUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _fragmentUniformBuffer = _factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<FragmentUniforms>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            // Initialize all view uniforms to identity matrix.
            //UpdateViewUniforms(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);

            UpdateResourceSet(ref drawParam.modulateParam);

            UpdateFragmentUniforms(drawParam.modulateParam);
            //ModulateType = drawParam.modulateParam.type; // Store modulate type here
        }
        private unsafe void UpdateIndexBuffer(FFLPrimitiveParam primitiveParam) // Unsafe: directly binds buffer
        {
            // Constructor must confirm that the index buffer/count here is not null.
            Debug.Assert(primitiveParam.pIndexBuffer != null && primitiveParam.indexCount > 0);

            // Set instance _indexCount to use later in Draw()
            _indexCount = primitiveParam.indexCount;

            _indexBuffer = _factory.CreateBuffer(new BufferDescription(
                _indexCount * sizeof(ushort),
                BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            IntPtr indexBufferPtr = (IntPtr)primitiveParam.pIndexBuffer;

            // TODO: UPDATE WITH COMMAND LIST...???
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, indexBufferPtr, _indexCount * sizeof(ushort));
        }

        /// <summary>
        /// Method to determine the pipeline based on the ModulateType.
        /// </summary>
        /// <param name="type">FFLModulateType indicating the draw type.</param>
        private void SetPipeline(FFLModulateType type)
        {
            // Choose pipeline based on what the draw param needs.

            // Faceline/mask 2D plane drawing:
            if (type > FFLModulateType.FFL_MODULATE_TYPE_MOLE) // Highest
                _pipeline = _resourceManager.PipelineFaceline2DPlane; // This is faceline
            else if (type > FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MAX - 1)
                _pipeline = _resourceManager.PipelineMask2DPlane; // This is mask, second highest

            // DrawXlu stage:

            else if (type == FFLModulateType.FFL_MODULATE_TYPE_SHAPE_GLASS)
                _pipeline = _resourceManager.PipelineShapeXluGlass; // No culling for glass
            else if (type > FFLModulateType.FFL_MODULATE_TYPE_SHAPE_CAP) // Highest Opa shape
                _pipeline = _resourceManager.PipelineShapeXlu;

            // DrawOpa stage:

            /*
            else if (drawParam.modulateParam.type == FFLModulateType.FFL_MODULATE_TYPE_SHAPE_HAIR)
                _pipeline = _resourceManager.PipelineShapeOpaHair; // Special case for extra hair attributes
            */
            else
                _pipeline = _resourceManager.PipelineShapeOpa; // Default
        }

        /// <summary>
        /// Initializes _vertexBuffer, then copies/interleaves all DrawParam attributes into
        /// a single byte array before updating it on the GraphicsDevice.
        /// </summary>
        /// <param name="param">FFLAttributeBufferParam containing pointers to all attributes.</param>
        /*
        private void UpdateVertexBufferSingle(FFLAttributeBufferParam param)
        {
            // Copy and interleave from the attribute pointers into one vertex buffer.
            byte[] vertexBuffer = VertexInterleaver.CopyInterleaveAttrBufToBytes(param,
                _pipeline == _resourceManager.PipelineMask2DPlane || _pipeline == _resourceManager.PipelineFaceline2DPlane);
            // Create the DeviceBuffer and update it.
            _vertexBuffer = _factory.CreateBuffer(new BufferDescription(
                sizeInBytes: (uint)vertexBuffer.Length,
                usage: BufferUsage.VertexBuffer));
            // TODO: UPDATE IN CommandList INSTEAD???
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertexBuffer);
        }
        */

        /// <summary>
        /// Initializes all _vertexBuffers, then updates each of them with
        /// all of the attribute buffers according to the pipeline's vertex layout.
        /// </summary>
        /// <param name="param">FFLAttributeBufferParam containing pointers to all attributes.</param>
        private unsafe void UpdateVertexBuffers(FFLAttributeBufferParam param)
        {
            // Iterate over each attribute buffer.

            // Does the pipeline need only position and texCoord?
            bool onlyPosTex = _pipeline.Equals(_resourceManager.PipelineMask2DPlane) || _pipeline.Equals(_resourceManager.PipelineFaceline2DPlane);

            /* Common missing attributes include:
             * normal   (stride = 4, 2D planes)
             * tangent  (stride = 4, not hair)
             * color    (stride = 4, not hair)
             * texCoord (stride = 8, hair)
             */
            // For 2D planes, normal and others can be substituted with texCoord.
            // For ones missing color and tangent, instead normal can be bound
            // and for hair missing texCoord, position will be bound.

            // Obtain vertex count in order to replace missing buffers.
            /*
            uint vertexCount = param.attributeBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION].size /
                               param.attributeBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION].stride;
            */

            // attribute buffer type you can reliably bind if the current attribute is missing
            FFLAttributeBuffer failsafeBuffer = param.attributeBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION];
            // ^^ I don't think there's a shape where index is 0 (which we have ensured it's not) and position is also empty.

            // NOTE: as a test this is stoppping at normal
            for (FFLAttributeBufferType type = 0; type < FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT; type++) // MAX; type++)
            {
                FFLAttributeBuffer attrBuffer = param.attributeBuffers[(int)type];

                _vertexBuffers[(int)type]?.Dispose(); // Dispose this buffer if it exists.

                // Do not assign anything over texCoord if the caller does not need it.
                if (onlyPosTex && type > FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD)
                    continue;

                // Does this buffer not have any data at all?
                if (!attrBuffer.IsUsable())
                    // Replace current buffer with one that is known to have data.
                    attrBuffer = failsafeBuffer;

                // Create a Veldrid buffer using the attribute buffer data directly.
                _vertexBuffers[(int)type] = _factory.CreateBuffer(new BufferDescription(
                    sizeInBytes: attrBuffer.size,
                    usage: BufferUsage.VertexBuffer));

                // Update the buffer with the data directly from the pointer
                _graphicsDevice.UpdateBuffer(_vertexBuffers[(int)type], 0, (IntPtr)attrBuffer.ptr, attrBuffer.size);

                // Bind the buffer to the corresponding slot (type index as slot)
                //_commandList.SetVertexBuffer((uint)type, _vertexBuffers[(int)type]);
            }
        }

        /// <summary>
        /// Creates or updates resource set with uniform buffers and a texture.
        /// </summary>
        /// <param name="texture">Texture to bind into the resource set.</param>
        public void UpdateResourceSet(FFLModulateType modulateType, Texture texture)
        {
            _textureView?.Dispose();
            _resourceSet?.Dispose(); // ^^ Dispose old resources if they already exist.

            // Set to instance TextureView so it can be disposed later.
            _textureView = _factory.CreateTextureView(texture);

            // Assign sampler based on shape type.
            Sampler sampler = modulateType switch
            {
                // Mirrored sampler:
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_FACELINE => _resourceManager.SamplerMirror,
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_CAP => _resourceManager.SamplerMirror,
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_GLASS => _resourceManager.SamplerMirror,
                _ => _resourceManager.Sampler // Clamp to edge by default.
            };

            // Create resource set
            _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                _resourceManager.ResourceLayout, // Use resource layout from resourceManager
                _vertexUniformBuffer,
                _fragmentUniformBuffer,
                _textureView,
                sampler));
        }
        /// <summary>
        /// Creates or updates resource set with uniform buffers and texture from the ModulateParam.
        /// </summary>
        /// <param name="modulateParam">FFLModulateParam</param>
        private void UpdateResourceSet(ref FFLModulateParam modulateParam)
        {
            // This will be assigned to either this shape's texture, or a placeholder blank texture.
            Texture texture = GetTextureIfExists(ref modulateParam);
            UpdateResourceSet(modulateParam.type, texture);
        }


        /// <summary>
        /// Determines whether or not a modulateParam is using a placeholder value for its texture, instead of an actual texture.
        /// TODO: may or may not remove this later.
        /// </summary>
        /// <returns>Is this modulateParam using a texture placeholder?</returns>
        private static unsafe bool IsModulateTextureUsingPlaceholder(FFLModulateParam modulateParam)
        {
            // Only faceline and mask texture2D pointers will ever use placeholders.
            if (modulateParam.type != FFLModulateType.FFL_MODULATE_TYPE_SHAPE_FACELINE
                && modulateParam.type != FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK)
                return false;
            // FFL_TEXTURE_PLACEHOLDER = macro defining what that placeholder is.
            return (modulateParam.pTexture2D == FFL.FFL_TEXTURE_PLACEHOLDER);
        }

        /// <summary>
        /// Either returns a view of the DrawParam texture, or the default texture view.
        /// </summary>
        /// <param name="modulateParam">FFLModulateParam</param>
        private unsafe Texture GetTextureIfExists(ref FFLModulateParam modulateParam)
        {
            // If an override texture is set, always use that.
            if (_overrideTexture != null)
            {
                return _overrideTexture;
            }

            // Attempt to get texture referenced in ModulateParam.
            if (modulateParam.pTexture2D != null
                // Make sure it is not using a placeholder either.
                && !IsModulateTextureUsingPlaceholder(modulateParam))
            {
                UIntPtr textureHandle = (UIntPtr)modulateParam.pTexture2D;
                // Try to get the texture for this texture handle.
                if (_textureManager.GetTextureFromMap(textureHandle, out Texture? texture) && texture != null)
                {
                    // Set ModulateParam and current texture handle to dispose and clear later.
                    _pModulateParam = (FFLModulateParam*)Unsafe.AsPointer(ref modulateParam);
                    _textureHandle = textureHandle;
                    return texture;
                }
                // Texture not found, this is not a normal circumstance
                Debug.Assert(false, $"Texture handle {textureHandle} not found. Was it already deleted?");
                Console.WriteLine($"Texture not found for handle: {textureHandle}. Using default texture.");
                // (Fall through)
            }
            // Use default texture if texture pointer is null
            return _resourceManager.DefaultTexture;
        }

        public void UpdateViewUniforms(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            // Multiply model and view to make ModelView
            _vertexUniforms.ModelView = Matrix4x4.Multiply(model, view);
            _vertexUniforms.Projection = projection;

            _graphicsDevice.UpdateBuffer(_vertexUniformBuffer, 0, ref _vertexUniforms);
        }

        /// <summary>
        /// Sets fragment uniforms based on modulate parameters.
        /// </summary>
        /// <param name="modulateParam">FFLModulateParam containing mode/type, const colors, etc.</param>
        private void UpdateFragmentUniforms(FFLModulateParam modulateParam)
        {
            _fragmentUniforms.ModulateMode = (int)modulateParam.mode;
            // Set constant colors if they exist.
            unsafe // Dereferencing color pointers.
            {
                // Directly cast FFLColor to System.Numerics.Vector4.
                _fragmentUniforms.ColorR = modulateParam.pColorR != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorR)
                    : RgbaFloat.Clear;
                _fragmentUniforms.ColorG = modulateParam.pColorG != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorG)
                    : RgbaFloat.Clear;
                _fragmentUniforms.ColorB = modulateParam.pColorB != null
                    ? new RgbaFloat(*(Vector4*)modulateParam.pColorB)
                    : RgbaFloat.Clear;
            }
            // Update fragment uniform buffer.
            _graphicsDevice.UpdateBuffer(_fragmentUniformBuffer, 0, ref _fragmentUniforms);
        }

        public void Draw(CommandList commandList)
        {
            // Set pipeline
            commandList.SetPipeline(_pipeline);

            // Set resources
            commandList.SetGraphicsResourceSet(0, _resourceSet);

            // Set index buffer
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            // Set single vertex buffer
            //commandList.SetVertexBuffer(0, _vertexBuffer);
            // Set multiple vertex buffers
            for (uint i = 0; i < _vertexBuffers.Length; i++)
            {
                if (_vertexBuffers[i] != null)
                    commandList.SetVertexBuffer(i, _vertexBuffers[i]);
            }

            // Submit draw command
            commandList.DrawIndexed(
                indexCount: _indexCount, // Set in UpdateIndexBuffer
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }


        public unsafe void Dispose()
        {
            GC.SuppressFinalize(this);
            // Dispose DeviceBuffers
            _indexBuffer.Dispose();
            // Dispose either single vertex buffer or all buffers if they exist.
            //_vertexBuffer?.Dispose();
            foreach (var buffer in _vertexBuffers)
            {
                buffer?.Dispose();
            }
            _vertexUniformBuffer.Dispose();
            _fragmentUniformBuffer.Dispose();

            // Dispose ResourceSet
            _resourceSet.Dispose();

            // Dispose texture view if it exists
            _textureView?.Dispose();
            // If texture handle is bound, and clear it from ModulateParam.
            if (_textureHandle != UIntPtr.Zero)
            {
                _textureManager.DisposeTextureHandle(_textureHandle);
                _textureHandle = UIntPtr.Zero; // Reset to zero.
                // Clear handle from ModulateParam.
                if (_pModulateParam != null)
                    _pModulateParam->pTexture2D = null;
            }

            // pipeline is managed by resourceManager
        }
    }
}
