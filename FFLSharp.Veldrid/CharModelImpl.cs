using FFLSharp.Interop;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Veldrid;

namespace FFLSharp.Veldrid
{
    public class CharModelImpl : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice; // Passed to DrawParamRenderer
        private readonly ICharModelResource _resourceManager;
        private readonly ITextureManager _textureManager;
        private readonly ResourceFactory _factory; // Used for making faceline/mask textures.

        // Corresponding to DrawOpa/DrawXlu lists.
        private readonly Dictionary<FFLModulateType, DrawParamGpuImpl> _opaParams = new();
        private readonly Dictionary<FFLModulateType, DrawParamGpuImpl> _xluParams = new();

        // Render textures that need to be initialized before the model is drawn:
        // (Usually instantiated in FFLInitCharModelGPUStep, which is skipped here.)

        // Faceline texture and framebuffer.
        private Texture _facelineTexture;
        private Framebuffer _facelineFramebuffer;
        private readonly List<DrawParamGpuImpl> _tmpParams = new(); // Temporary DrawParamRenderer instances for texture drawing.
        // Mask textures and framebuffers, one for each expression.
        private readonly Texture[] _maskTextures = new Texture[(int)FFLExpression.FFL_EXPRESSION_MAX];
        private readonly Framebuffer[] _maskFramebuffers = new Framebuffer[(int)FFLExpression.FFL_EXPRESSION_MAX];
        // ^^ Not all of these will be used or allocated to!

        // Current expression, controls which mask is active.
        public FFLExpression CurrentExpression { get; private set; } = FFLExpression.FFL_EXPRESSION_NORMAL;

        // CharModel field used for FFL calls such as FFLSetExpression.
        unsafe private readonly FFLCharModel* _pCharModel;

        public CharModelImpl(GraphicsDevice graphicsDevice, ICharModelResource resourceManager,
            ITextureManager textureManager, ResourceFactory factory, /*CommandList commandList,*/ ref FFLCharModel charModel)
        {
            _graphicsDevice = graphicsDevice;
            _resourceManager = resourceManager;
            _textureManager = textureManager;
            _factory = factory; // Will also create and submit a new CommandList.
            Debug.Assert(_resourceManager.SwapchainTexFormat != null); // ResourceManager needs to be instantiated

            unsafe
            {
                _pCharModel = (FFLCharModel*)Unsafe.AsPointer(ref charModel);
            }

            // Create and render mask and faceline textures.
            CreateRenderTextures(ref charModel, _resourceManager.SwapchainTexFormat.Value);

            // NOTE: LEAK CASE:
            // (basicallly bc we are ignoring ffl requests to delete textures, we NEED to initialize charmodel textures always

            // Use a new CommandList for this:
            CommandList commandList = _factory.CreateCommandList();
            commandList.Begin();
            commandList.PushDebugGroup("Render Mask and Faceline Textures");
            DrawRenderTextures(ref charModel, commandList, _tmpParams);
            commandList.PopDebugGroup();
            commandList.End();
            _graphicsDevice.SubmitCommands(commandList); // Submit command list.
            commandList.Dispose(); // Not needed anymore
            DisposeRenderTexturesTempResources();

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
                var renderer = new DrawParamGpuImpl(_graphicsDevice, _resourceManager,
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
            Texture texture = _maskTextures[(int)expression]; // Get mask texture for this expression
            Debug.Assert(texture != null); // Make sure that texture isn't null somehow...
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
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_FACELINE => _facelineTexture,// Will be null if no faceline texture, which is okay.
                FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MASK => _maskTextures[(int)CurrentExpression],
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

        #region Faceline and Mask Texture Drawing
        /// <summary>
        /// Initializes framebuffers and textures for faceline and masks.
        /// </summary>
        /// <param name="textureResolution">(uint)FFLiCharModel.charModelDesc.resolution</param>
        /// <param name="pixelFormat">Desired pixel format for the render textures, needs transparency.</param>
        private unsafe void CreateRenderTextures(ref FFLCharModel charModel, PixelFormat pixelFormat)
        {
            // Need to get texture resolution and active masks from FFLiCharModel
            FFLiCharModel* pModel = (FFLiCharModel*)Unsafe.AsPointer(ref charModel);
            // There is only one faceline texture and framebuffer.
            // Separate the actual resolution from the mipmap enable mask (which is probably never enabled but eh)
            uint textureResolution = (uint)(pModel->charModelDesc.resolution & FFLResolution.FFL_RESOLUTION_MASK);

            // Ensure faceline texture is meant to be rendered.
            if (pModel->facelineRenderTexture.pTexture2D != null) // valid value: 0x01/FFL.FFL_TEXTURE_PLACEHOLDER
            {
                uint halfResolution = textureResolution / 2; // Faceline texture width is half
                _facelineTexture = _factory.CreateTexture(
                    TextureDescription.Texture2D(
                        halfResolution, textureResolution, 1, 1, // Width, Height, MipLevels, SampleCount
                                                                 // Usually, the pixel format in the faceline/mask pipeline is the swapchain pixel format.
                        pixelFormat,                             // Desired pixel format
                        TextureUsage.RenderTarget | TextureUsage.Sampled)); // Need to render to this
                _facelineFramebuffer = _factory.CreateFramebuffer(
                    new FramebufferDescription(depthTarget: null, colorTargets: _facelineTexture));
            }

            // Need to only create the faceline textures that are necessary.
            // Iterate through every FFLiRenderTexture pointer in this array:
            for (int i = 0; i < (int)FFLExpression.FFL_EXPRESSION_MAX; i++)
            {
                // The value of this when active will be 0x01 (FFL.FFLI_RENDER_TEXTURE_PLACEHOLDER)
                if (pModel->maskTextures.pRenderTextures[i] == null)
                    continue;

                // Create mask texture and framebuffer for expression i.
                _maskTextures[i] = _factory.CreateTexture(
                    TextureDescription.Texture2D(
                        // Aspect ratio 1:1
                        textureResolution, textureResolution, 1, 1, // Width, Height, MipLevels, SampleCount
                        pixelFormat,                                // Desired pixel format
                        TextureUsage.RenderTarget | TextureUsage.Sampled)); // Need to render to this
                _maskFramebuffers[i] = _factory.CreateFramebuffer(
                    new FramebufferDescription(depthTarget: null, colorTargets: _maskTextures[i]));
            }

            CurrentExpression = pModel->expression; // Set current expression in instance.
            /*
            int expression = (int)pModel->expression;
            FFLDrawParam* pMaskDrawParam = FFL.GetDrawParamXluMask((FFLCharModel*)Unsafe.AsPointer(ref charModel)); // usually returned as const
            var maskTextureHandle = _textureManager.AddTextureToMap(_maskTextures[expression]);
            pMaskDrawParam->modulateParam.pTexture2D = (void*)maskTextureHandle;
            */

            // Framebuffers are ready to bind and textures are ready to use.
        }

        private unsafe void DrawRenderTextures(ref FFLCharModel charModel, CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            // Need to access fields from FFLiCharModel.
            FFLiCharModel* pModel = (FFLiCharModel*)Unsafe.AsPointer(ref charModel);

            FFLiTextureTempObject* pTmpObject = pModel->pTextureTempObject;

            // Draw into the faceline texture if it is meant to be rendered.
            if (_facelineFramebuffer != null)
            {
                // Get temp object that hosts faceline parts.
                FFLiFacelineTextureTempObject* pFaceTmpObject = &pTmpObject->facelineTexture;
                // Get faceline color to use as the clear color/background.
                FFLColor facelineColor = FFL.GetFacelineColor(pModel->charInfo.parts.facelineColor);
                commandList.PushDebugGroup("Draw Faceline Texture");
                DrawFacelineParts(_facelineFramebuffer, pFaceTmpObject, facelineColor, commandList, tmpParams);
                commandList.PopDebugGroup();
                // Delete DrawParams
            }

            // Loop through mask textures and render active ones.
            for (int i = 0; i < (int)FFLExpression.FFL_EXPRESSION_MAX; i++)
            {
                if (_maskFramebuffers[i] == null)
                    continue;

                // Container for mask DrawParams.
                FFLiRawMaskDrawParam* pDrawParam = pTmpObject->maskTextures.pRawMaskDrawParam[i];
                commandList.PushDebugGroup($"Draw Mask {(FFLExpression)i}");
                DrawMaskParts(_maskFramebuffers[i], pDrawParam, commandList, tmpParams); // i = current expression
                commandList.PopDebugGroup();
            }

            // (Cleanup will be done after command list is submitted.)
        }

        private unsafe void DisposeRenderTexturesTempResources()
        {
            foreach (var renderer in _tmpParams)
            {
                renderer.Dispose(); // Dispose all temporary instances after submission.
            }
            _tmpParams.Clear(); // Clear all DrawParams from the list.
            // Call FFL methods to delete DrawParams.
            FFLiCharModel* pModel = (FFLiCharModel*)_pCharModel;
            if (_facelineFramebuffer != null)
                FFL.iDeleteTempObjectFacelineTexture(&pModel->pTextureTempObject->facelineTexture,
                    &pModel->charInfo, pModel->charModelDesc.resourceType);
            FFL.iDeleteTempObjectMaskTextures(&pModel->pTextureTempObject->maskTextures,
                pModel->charModelDesc.allExpressionFlag, pModel->charModelDesc.resourceType);
            FFL.iDeleteTextureTempObject(pModel);
        }

        /// <summary>
        /// Binds the faceline texture and draws the parts.
        /// </summary>
        /// <param name="pTmpObject">FFLiCharModel.pTextureTempObject</param
        /// <param name="facelineColor">FFL.GetFacelineColor(pCharModel->charInfo.parts.facelineColor)</param>
        private unsafe void DrawFacelineParts(Framebuffer framebuffer, FFLiFacelineTextureTempObject* pFaceTmpObject,
            FFLColor facelineColor, CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            // Prepare CommandList to draw into the faceline texture.
            commandList.SetFramebuffer(framebuffer);
            commandList.ClearColorTarget(0, new RgbaFloat(*(Vector4*)&facelineColor)); // Cast from FFLColor

            // Called in FFLiRenderFacelineTexture before doing any drawing:
            FFL.iInvalidateTempObjectFacelineTexture(pFaceTmpObject);

            // Basically FFLiDrawFacelineTexture
            DrawFromDrawParamOnce(ref pFaceTmpObject->drawParamFaceMake, pFaceTmpObject->pTextureFaceMake, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pFaceTmpObject->drawParamFaceLine, pFaceTmpObject->pTextureFaceLine, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pFaceTmpObject->drawParamFaceBeard, pFaceTmpObject->pTextureFaceBeard, commandList, tmpParams);

            // Usually checking if the texture is null is not a good idea in this specific instance
            // because we are specifically setting pTexture2D on draw params to null when we free them
            // but we only freed the texture, FFL will see that and assume the entire thing is freed.
            // BUT for faceline textures, the pTexture2D to check is separate from the DrawParam so should be ok
        }

        /// <summary>
        /// Binds the specified mask texture and draws the parts.
        /// </summary>
        /// <param name="framebuffer">Framebuffer for the current mask.</param>
        /// <param name="pMaskTmpObject">pMaskTmpObject->pRawMaskDrawParam[(int)expression]</param>
        private unsafe void DrawMaskParts(Framebuffer framebuffer, FFLiRawMaskDrawParam* pDrawParam,
            CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            // Prepare CommandList to draw into the current mask texture.
            commandList.SetFramebuffer(framebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Clear);

            FFL.iInvalidateRawMask(pDrawParam); // Invalidation happens before drawing.

            // Conditionally draw all mask params if their texture is not null.
            // Eyes and mouth are always present.
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsMustache[0],
                pDrawParam->drawParamRawMaskPartsMustache[0].primitiveParam.indexCount, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsMustache[1],
                pDrawParam->drawParamRawMaskPartsMustache[1].primitiveParam.indexCount, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsMouth, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsEyebrow[0],
                pDrawParam->drawParamRawMaskPartsEyebrow[0].primitiveParam.indexCount, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsEyebrow[1],
                pDrawParam->drawParamRawMaskPartsEyebrow[1].primitiveParam.indexCount, commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsEye[0], commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsEye[1], commandList, tmpParams);
            DrawFromDrawParamOnce(ref pDrawParam->drawParamRawMaskPartsMole,
                pDrawParam->drawParamRawMaskPartsMole.primitiveParam.indexCount, commandList, tmpParams);
            // ^^ Basically FFLiDrawRawMask
        }

        private void DrawFromDrawParamOnce(ref FFLDrawParam param, CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            // Not meant for shapes because no view uniforms are ever set.
            Debug.Assert(param.modulateParam.type > FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MAX - 1);
            // Create and use this DrawParamRenderer once.
            DrawParamGpuImpl renderer = new(_graphicsDevice, _resourceManager, _textureManager, ref param);
            // Assuming CommandList is ready for drawing, and doesn't
            // require view uniforms to be set (so, for 2D planes)
            renderer.Draw(commandList);
            // Dispose the DrawParamRenderer - only needed for one draw.
            //renderer.Dispose(); // Never mind, cannot do this!!!
            // Actually, add it to a list so that it can be disposed of later.
            tmpParams.Add(renderer);
        }
        /// <summary>
        /// This overload additionally will only continue if ptrMustNotBeNull is not null.
        /// </summary>
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, void* ptrMustNotBeNull, CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            if (ptrMustNotBeNull != null)
                DrawFromDrawParamOnce(ref param, commandList, tmpParams);
        }

        /// <summary>
        /// This overload additionally will only continue if mustNotBeZero is not zero (index count?).
        /// </summary>
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, uint mustNotBeZero, CommandList commandList, List<DrawParamGpuImpl> tmpParams)
        {
            if (mustNotBeZero != 0)
                DrawFromDrawParamOnce(ref param, commandList, tmpParams);
        }

        #endregion


        public void Dispose()
        {
            GC.SuppressFinalize(this);

            // Clean up faceline and mask textures and framebuffers.
            _facelineFramebuffer?.Dispose();
            _facelineTexture?.Dispose();
            foreach (var framebuffer in _maskFramebuffers)
            {
                framebuffer?.Dispose();
            }
            foreach (var texture in _maskTextures)
            {
                texture?.Dispose();
            }

            // Should already be done, but just in case:
            foreach (var renderer in _tmpParams)
            {
                // List should be clear if this was disposed properly.
                renderer.Dispose();
            }

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
