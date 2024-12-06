using FFLSharp.Interop;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    public class CharModelImpl : IDisposable
    {
        private GraphicsDevice _graphicsDevice; // Passed to DrawParamRenderer
        private ICharModelResource _resourceManager;
        private TextureManager _textureManager;
        private ResourceFactory _factory; // Used for making faceline/mask textures.

        // Corresponding to DrawOpa/DrawXlu lists.
        private readonly Dictionary<FFLModulateType, DrawParamGpuHandler> _opaParams = new Dictionary<FFLModulateType, DrawParamGpuHandler>();
        private readonly Dictionary<FFLModulateType, DrawParamGpuHandler> _xluParams = new Dictionary<FFLModulateType, DrawParamGpuHandler>();

        // Render textures that need to be initialized before the model is drawn:
        // (Usually instantiated in FFLInitCharModelGPUStep, which is skipped here.)
        /*
        // Faceline texture and framebuffer.
        private Texture FacelineTexture;
        private Framebuffer _facelineFramebuffer;
        private readonly List<DrawParamGpuHandler> _tmpParams = new List<DrawParamGpuHandler>(); // Temporary DrawParamRenderer instances for texture drawing.
        // Mask textures and framebuffers, one for each expression.
        private readonly Texture[] MaskTextures = new Texture[(int)FFLExpression.FFL_EXPRESSION_MAX];
        private readonly Framebuffer[] _maskFramebuffers = new Framebuffer[(int)FFLExpression.FFL_EXPRESSION_MAX];
        // ^^ Not all of these will be used or allocated to!
        */
        private CharModelTexturesRenderer _modelTex;

        // Current expression, controls which mask is active.
        public FFLExpression CurrentExpression { get; private set; } = FFLExpression.FFL_EXPRESSION_NORMAL;

        // CharModel field used for FFL calls such as FFLSetExpression.
        unsafe private FFLCharModel* _pCharModel;

        public CharModelImpl(GraphicsDevice graphicsDevice, ICharModelResource resourceManager,
            TextureManager textureManager, ResourceFactory factory, /*CommandList commandList,*/ ref FFLCharModel charModel)
        {
            Initialize(graphicsDevice, resourceManager, textureManager, factory, ref charModel);
        }

        public void Initialize(GraphicsDevice graphicsDevice, ICharModelResource resourceManager,
            TextureManager textureManager, ResourceFactory factory, ref FFLCharModel charModel)
        {
            _graphicsDevice = graphicsDevice;
            _resourceManager = resourceManager;
            _textureManager = textureManager;
            _factory = factory; // Will also create and submit a new CommandList.
            Debug.Assert(_resourceManager.SwapchainTexFormat != null); // ResourceManager needs to be instantiated

            unsafe
            {
                fixed (FFLCharModel* pCharModel = &charModel)
                {
                    _pCharModel = pCharModel;

                    // Also ensure that this CharModel has been set up.
                    Debug.Assert(((FFLiCharModel*)pCharModel)->charModelDesc.resolution != 0,
                        "CharModel texResolution is 0, suggesting it is not set up (all zeroes).");
                }

                // Set current expression in instance.
                CurrentExpression = ((FFLiCharModel*)_pCharModel)->expression; // Set current expression in instance.
            }

            // Create and render mask and faceline textures.
            //CreateRenderTextures(ref charModel, _resourceManager.SwapchainTexFormat.Value);
            _modelTex = new(_graphicsDevice, _resourceManager, _textureManager, _factory, ref charModel);

            // Add shape draw params to _opaParams and _xluParams (call FFLGetDrawParam*)
            InitializeDrawParams(ref charModel);
        }

        private unsafe void InitializeDrawParams(ref FFLCharModel charModel)
        {
            // DrawOpa stage
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaFaceline((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaBeard((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaNose((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaForehead((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaHair((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamOpaCap((FFLCharModel*)Unsafe.AsPointer(ref model)));
            // DrawXlu stage
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamXluMask((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamXluNoseLine((FFLCharModel*)Unsafe.AsPointer(ref model)));
            AddDrawParam(ref charModel, model => (IntPtr)FFL.GetDrawParamXluGlass((FFLCharModel*)Unsafe.AsPointer(ref model)));

            // make sure this one here is present bc we update it later
            Debug.Assert(_xluParams.GetValueOrDefault(FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK) != null);

            // Delete CharModel instance. This only contains shape data that was already uploaded.
            FFL.DeleteCharModel((FFLCharModel*)Unsafe.AsPointer(ref charModel));
        }

        private unsafe void AddDrawParam(ref FFLCharModel charModel, Func<FFLCharModel, IntPtr> getDrawParamFunc)
        {
            IntPtr drawParamPtr = getDrawParamFunc(charModel);
            // Cast this pointer to FFLDrawParam.
            FFLDrawParam* pDrawParam = (FFLDrawParam*)drawParamPtr;

            if (pDrawParam != null && pDrawParam->primitiveParam.indexCount != 0)
            { // Note: if the index count is 0, this shape is empty.
                FFLModulateType modulateType = pDrawParam->modulateParam.type; // Key in dictionary
                var renderer = new DrawParamGpuHandler(_graphicsDevice, _resourceManager,
                    _textureManager, ref *pDrawParam, GetOverrideTexture(modulateType)); // Dereference DrawParam here

                // Determine if the draw param is in the DrawOpa group based on its modulate type.
                if (modulateType < FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK) // Less than mask? (cap, hair...)
                    _opaParams.Add(modulateType, renderer);
                else // Assuming everything that's not Opa is Xlu.
                    _xluParams.Add(modulateType, renderer);
            }

        }

        public unsafe void SetExpression(FFLExpression expression)
        {
            // Get mask texture for this expression.
            Texture texture = _modelTex.MaskTextures[(int)expression]
                // If the mask was not created, texture will be null.
                ?? throw new ExpressionNotSet(expression); // Don't bind a non-existent texture

            FFL.SetExpression(_pCharModel, expression); // Set expression with FFL.
            // Update the resource set to change the mask texture.
            _xluParams[FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK]
                .UpdateResourceSet(FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK, texture);
        }

        /// <summary>
        /// Gets which texture a DrawParam instance with, or nothing.
        /// </summary>
        /// <param name="type">pDrawParam->modulateParam.type</param>
        /// <returns>Either the texture to override with or null.</returns>
        private Texture? GetOverrideTexture(FFLModulateType type)
        {
            return type switch
            {
                // Will be null if no faceline texture, which is okay.
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_FACELINE => _modelTex.FacelineTexture, // May be null.
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK => _modelTex.MaskTextures[(int)CurrentExpression],
                _ => null,
            };
        }

        public void UpdateViewUniforms(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            foreach (var renderer in _opaParams)
            {
                renderer.Value.UpdateViewUniforms(model, view, projection);
            }

            foreach (var renderer in _xluParams)
            {
                renderer.Value.UpdateViewUniforms(model, view, projection);
            }
        }

        public void Draw(CommandList commandList)
        {
            DrawOpa(commandList);
            DrawXlu(commandList);
        }

        public void DrawOpa(CommandList commandList)
        {
            commandList.PushDebugGroup("DrawOpa Stage");
            foreach (var renderer in _opaParams)
            {
                commandList.PushDebugGroup($"DrawOpa: {renderer.Key}");
                renderer.Value.Draw(commandList);
                commandList.PopDebugGroup();
            }
            commandList.PopDebugGroup();
        }
        public void DrawXlu(CommandList commandList)
        {
            commandList.PushDebugGroup("DrawXlu Stage");
            // Draw translucent renderers second
            foreach (var renderer in _xluParams)
            {
                commandList.PushDebugGroup($"DrawXlu: {renderer.Key}");
                renderer.Value.Draw(commandList);
                commandList.PopDebugGroup();
            }
            commandList.PopDebugGroup();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // Clean up faceline and mask textures and framebuffers.
            _modelTex.Dispose();

            // Dispose DrawParamRenderers
            foreach (var renderer in _opaParams)
            {
                renderer.Value.Dispose();
            }
            _opaParams.Clear();
            foreach (var renderer in _xluParams)
            {
                renderer.Value.Dispose();
            }
            _xluParams.Clear();
        }
    }
}
