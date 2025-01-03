using FFLSharp.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    public class CharModelRenderer : IDisposable
    {
        private GraphicsDevice _graphicsDevice; // Passed to DrawParamRenderer
        private IPipelineProvider _pipelineProvider;
        private TextureManager _textureManager;
        private ResourceFactory _factory; // Used for making faceline/mask textures.

        // Corresponding to DrawOpa/DrawXlu lists.
        private readonly Dictionary<FFLModulateType, DrawParamGpuBuffer> _opaParams = new Dictionary<FFLModulateType, DrawParamGpuBuffer>();
        private readonly Dictionary<FFLModulateType, DrawParamGpuBuffer> _xluParams = new Dictionary<FFLModulateType, DrawParamGpuBuffer>();

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

        public FFLCharModel CharModel;// { get; private set; }

        public CharModelRenderer(GraphicsDevice graphicsDevice, IPipelineProvider pipelineProvider,
            TextureManager textureManager, ResourceFactory factory, /*CommandList commandList,*/ ref FFLCharModel charModel)
        {
            unsafe
            {
                Initialize(graphicsDevice, pipelineProvider, textureManager, factory, (FFLCharModel*)Unsafe.AsPointer(ref charModel));
            }
        }

        public CharModelRenderer(GraphicsDevice graphicsDevice, IPipelineProvider pipelineProvider,
            TextureManager textureManager, ResourceFactory factory, CharModelInitParam param)
        {
            FFLCharModel charModel = FFLManager.CreateCharModel(param, textureManager);
            CharModel = charModel;
            unsafe
            {
                Initialize(graphicsDevice, pipelineProvider, textureManager, factory, &charModel);
            }
        }
/*
        public void Initialize(GraphicsDevice graphicsDevice, IPipelineProvider pipelineProvider,
            TextureManager textureManager, ResourceFactory factory, ref FFLCharModel charModel)
        {
            _graphicsDevice = graphicsDevice;
            _pipelineProvider = pipelineProvider;
            _textureManager = textureManager;
            _factory = factory; // Will also create and submit a new CommandList.

            CharModel = charModel; // Set instance of CharModel.
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

            // NOTE: LEAK CASE:
            // because we are ignoring ffl requests to delete textures...
            // ... we NEED to initialize charmodel textures always

            // Create and render mask and faceline textures.
            //CreateRenderTextures(ref charModel, _pipelineProvider.SwapchainTexFormat.Value);
            _modelTex = new CharModelTexturesRenderer(_graphicsDevice, _pipelineProvider, _textureManager, _factory, ref charModel);

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
            Debug.Assert(_xluParams.TryGetValue(FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK, out DrawParamGpuBuffer _));

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
                var renderer = new DrawParamGpuBuffer(_graphicsDevice, _pipelineProvider,
                    _textureManager, pDrawParam, GetOverrideTexture(modulateType)); // Dereference DrawParam here

                // Determine if the draw param is in the DrawOpa group based on its modulate type.
                if (modulateType < FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK) // Less than mask? (cap, hair...)
                    _opaParams.Add(modulateType, renderer);
                else // Assuming everything that's not Opa is Xlu.
                    _xluParams.Add(modulateType, renderer);
            }

        }
*/
        public unsafe void Initialize(GraphicsDevice graphicsDevice, IPipelineProvider pipelineProvider,
            TextureManager textureManager, ResourceFactory factory, FFLCharModel* pCharModel)
        {
            // Store references to the passed parameters
            _graphicsDevice = graphicsDevice;
            _pipelineProvider = pipelineProvider;
            _textureManager = textureManager;
            _factory = factory;

            // Ensure the CharModel pointer is valid and assign it
            Debug.Assert(pCharModel != null, "pCharModel is null.");
            Debug.Assert(((FFLiCharModel*)pCharModel)->charModelDesc.resolution != 0, "CharModel resolution is 0, suggesting it is not set up.");

            // Copy CharModel data into the managed instance for safer access later.
            CharModel = *pCharModel;

            // Set the current expression from the unmanaged structure
            CurrentExpression = ((FFLiCharModel*)pCharModel)->expression;

            // Initialize textures for the CharModel
            _modelTex = new CharModelTexturesRenderer(_graphicsDevice, _pipelineProvider, _textureManager, _factory, pCharModel);

            // Extract and initialize draw parameters from the CharModel
            InitializeDrawParams(pCharModel);
        }

        private unsafe void InitializeDrawParams(FFLCharModel* pCharModel)
        {
            // Extract and add DrawParams for Opaque (Opa) rendering
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaFaceline(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaBeard(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaNose(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaForehead(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaHair(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamOpaCap(pCharModel));

            // Extract and add DrawParams for Translucent (Xlu) rendering
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamXluMask(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamXluNoseLine(pCharModel));
            AddDrawParam(pCharModel, () => (IntPtr)FFL.GetDrawParamXluGlass(pCharModel));

            // Ensure a key parameter is present for later updates
            Debug.Assert(_xluParams.TryGetValue(FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK, out DrawParamGpuBuffer _),
                "Shape mask draw param is missing.");

            // Delete the unmanaged CharModel only after all data is extracted
            FFL.DeleteCharModel(pCharModel);
        }

        private unsafe void AddDrawParam(FFLCharModel* pCharModel, Func<IntPtr> getDrawParamFunc)
        {
            IntPtr drawParamPtr = getDrawParamFunc();

            if (drawParamPtr != IntPtr.Zero)
            {
                FFLDrawParam* pDrawParam = (FFLDrawParam*)drawParamPtr;

                if (pDrawParam->primitiveParam.indexCount != 0)
                {
                    // Extract modulation type for categorizing DrawParams
                    FFLModulateType modulateType = pDrawParam->modulateParam.type;

                    // Create a GPU buffer for rendering the parameter
                    var renderer = new DrawParamGpuBuffer(
                        _graphicsDevice, _pipelineProvider, _textureManager, pDrawParam, GetOverrideTexture(modulateType));

                    // Categorize into Opa or Xlu based on modulation type
                    if (modulateType < FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK)
                        _opaParams.Add(modulateType, renderer); // Add to Opa
                    else
                        _xluParams.Add(modulateType, renderer); // Add to Xlu
                }
            }
        }

        public unsafe void SetExpression(FFLExpression expression)
        {
            // Get mask texture for this expression.
            Texture texture = _modelTex.MaskTextures[(int)expression]
                // If the mask was not created, texture will be null.
                ?? throw new ExpressionNotSet(expression); // Don't bind a non-existent texture

            fixed (FFLCharModel* pCharModel = &CharModel)
            {
                FFL.SetExpression(pCharModel, expression); // Set expression with FFL.
            }

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
                //Console.WriteLine($"deleting opa: {renderer.Key}");
                renderer.Value.Dispose();
            }
            _opaParams.Clear();
            foreach (var renderer in _xluParams)
            {
                //Console.WriteLine($"deleting xlu: {renderer.Key}");
                renderer.Value.Dispose();
            }
            _xluParams.Clear();
        }
    }
}
