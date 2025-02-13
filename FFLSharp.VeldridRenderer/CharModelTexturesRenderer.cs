using FFLSharp.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    /// <summary>
    /// Handles creating and rendering faceline and mask textures.
    /// Maintains ownership of the textures, releases the framebuffers when finished.
    /// </summary>
    class CharModelTexturesRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice; // Passed to DrawParamRenderer
        private readonly IPipelineProvider _pipelineProvider;
        private readonly TextureManager _textureManager;

        private readonly ResourceFactory _factory; // Used for creating faceline/mask textures.

        // Faceline and mask textures.
        public Texture? FacelineTexture;
        // There is one mask texture for each expression.
        public readonly Texture[] MaskTextures = new Texture[(int)FFLExpression.FFL_EXPRESSION_MAX];

        // Temporary DrawParamRenderer instances for texture drawing.
        private readonly List<DrawParamGpuBuffer> _tmpParams = new List<DrawParamGpuBuffer>();

        // Framebuffers are disposed after drawing is finished.
        private readonly Framebuffer[] _maskFramebuffers = new Framebuffer[(int)FFLExpression.FFL_EXPRESSION_MAX];
        // ^^ Not all of these will be used or allocated to.
        private Framebuffer? _facelineFramebuffer; // Not all CharModels require a faceline texture.

        // FFLiCharModel instance casted from FFLCharModel.
        unsafe private readonly FFLiCharModel* _pCharModel;
        // ^^ Accessed: texture resolution, active masks,

        public unsafe CharModelTexturesRenderer(GraphicsDevice graphicsDevice, IPipelineProvider pipelineProvider,
            TextureManager textureManager, ResourceFactory factory, FFLCharModel* pCharModel)
        {
            _graphicsDevice = graphicsDevice;
            _pipelineProvider = pipelineProvider;
            _textureManager = textureManager;
            _factory = factory; // Will also create and submit a new CommandList.

            // Make sure that the pipeline provider is instantiated with this:
            Debug.Assert(_pipelineProvider.SwapchainTexFormat != null);

            // Set _pCharModel by casting from FFLCharModel.
            _pCharModel = (FFLiCharModel*)pCharModel;

            // Create and render mask and faceline textures.
            CreateRenderTextures(_pCharModel, _pipelineProvider.SwapchainTexFormat.Value);

            // Begin drawing to faceline and mask textures.

            // Use a new CommandList for this:
            CommandList commandList = _factory.CreateCommandList();
            commandList.Begin();
            commandList.PushDebugGroup("Render Mask and Faceline Textures");

            // Below will bind framebuffers and draw to each texture.
            DrawRenderTextures(_pCharModel, commandList, _tmpParams);

            commandList.PopDebugGroup();
            commandList.End();
            _graphicsDevice.SubmitCommands(commandList); // Submit command list.
            commandList.Dispose(); // Dispose of the CommandList.

            DisposeRenderTexturesTempResources();
        }

        /// <summary>
        /// Initializes framebuffers and textures for faceline and masks.
        /// </summary>
        /// <param name="textureResolution">(uint)FFLiCharModel.charModelDesc.resolution</param>
        /// <param name="pixelFormat">Desired pixel format for the render textures, needs transparency.</param>
        private unsafe void CreateRenderTextures(FFLiCharModel* pModel, PixelFormat pixelFormat)
        {
            // There is only one faceline texture and framebuffer.
            // Separate the actual resolution from the mipmap enable mask (which is probably never enabled but eh)
            uint textureResolution = (uint)(pModel->charModelDesc.resolution & FFLResolution.FFL_RESOLUTION_MASK);

            // Ensure faceline texture is meant to be rendered.
            if (pModel->facelineRenderTexture.pTexture2D != null) // valid value: 0x01/FFL.FFL_TEXTURE_PLACEHOLDER
            {
                uint halfResolution = textureResolution / 2; // Faceline texture width is half
                FacelineTexture = _factory.CreateTexture(
                    TextureDescription.Texture2D(
                        halfResolution, textureResolution, 1, 1, // Width, Height, MipLevels, ArrayLayers
                                                                 // Usually, the pixel format in the faceline/mask pipeline is the swapchain pixel format.
                        pixelFormat,                             // Desired pixel format
                        TextureUsage.RenderTarget | TextureUsage.Sampled)); // Need to render to this
                _facelineFramebuffer = _factory.CreateFramebuffer(
                    new FramebufferDescription(depthTarget: null, colorTargets: FacelineTexture));
            }

            // Need to only create the faceline textures that are necessary.
            // Iterate through every FFLiRenderTexture pointer in this array:
            for (int i = 0; i < (int)FFLExpression.FFL_EXPRESSION_MAX; i++)
            {
                // The value of this when active will be 0x01 (FFL.FFLI_RENDER_TEXTURE_PLACEHOLDER)
                if (pModel->maskTextures.pRenderTextures[i] == null)
                    continue;

                // Create mask texture and framebuffer for expression i.
                MaskTextures[i] = _factory.CreateTexture(
                    TextureDescription.Texture2D(
                        // Aspect ratio 1:1
                        textureResolution, textureResolution, 1, 1, // Width, Height, MipLevels, ArrayLayers
                        pixelFormat,                                // Desired pixel format
                        TextureUsage.RenderTarget | TextureUsage.Sampled)); // Need to render to this
                _maskFramebuffers[i] = _factory.CreateFramebuffer(
                    new FramebufferDescription(depthTarget: null, colorTargets: MaskTextures[i]));
            }

            // Framebuffers are ready to bind and textures are ready to use.
        }

        private unsafe void DrawRenderTextures(FFLiCharModel* pModel, CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
        {
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

            // Called in FFLiRenderMaskTextures before looping:
            FFL.iInvalidatePartsTextures(&pTmpObject->maskTextures.partsTextures);
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
                //Console.WriteLine($"deleting tmp mask obj: {renderer.ModulateType}");
                renderer.Dispose(); // Dispose all temporary instances after submission.
            }
            _tmpParams.Clear(); // Clear all DrawParams from the list.
            // Call FFL methods to delete DrawParams.

            if (_facelineFramebuffer != null)
                FFL.iDeleteTempObjectFacelineTexture(&_pCharModel->pTextureTempObject->facelineTexture,
                    &_pCharModel->charInfo, _pCharModel->charModelDesc.resourceType);
            FFL.iDeleteTempObjectMaskTextures(&_pCharModel->pTextureTempObject->maskTextures,
                _pCharModel->charModelDesc.allExpressionFlag, _pCharModel->charModelDesc.resourceType);
            FFL.iDeleteTextureTempObject(_pCharModel);
        }

        /// <summary>
        /// Binds the faceline texture and draws the parts.
        /// </summary>
        /// <param name="pTmpObject">FFLiCharModel.pTextureTempObject</param
        /// <param name="facelineColor">FFL.GetFacelineColor(pCharModel->charInfo.parts.facelineColor)</param>
        private unsafe void DrawFacelineParts(Framebuffer framebuffer, FFLiFacelineTextureTempObject* pFaceTmpObject,
            FFLColor facelineColor, CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
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
            CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
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

        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
        {
            // Not meant for shapes because no view uniforms are ever set.
            Debug.Assert(param.modulateParam.type > FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MAX - 1);
            // Create and use this DrawParamRenderer once.
            FFLDrawParam* pDrawParam = (FFLDrawParam*)Unsafe.AsPointer(ref param);
            DrawParamGpuBuffer renderer = new DrawParamGpuBuffer(_graphicsDevice, _pipelineProvider, _textureManager, pDrawParam);
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
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, void* ptrMustNotBeNull, CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
        {
            if (ptrMustNotBeNull != null)
                DrawFromDrawParamOnce(ref param, commandList, tmpParams);
        }

        /// <summary>
        /// This overload additionally will only continue if mustNotBeZero is not zero (index count?).
        /// </summary>
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, uint mustNotBeZero, CommandList commandList, List<DrawParamGpuBuffer> tmpParams)
        {
            if (mustNotBeZero != 0)
                DrawFromDrawParamOnce(ref param, commandList, tmpParams);
        }

        private void DisposeFramebuffers()
        {
            _facelineFramebuffer?.Dispose();
            foreach (var framebuffer in _maskFramebuffers)
            {
                framebuffer?.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            // Clean up faceline and mask textures.
            DisposeFramebuffers();

            FacelineTexture?.Dispose();

            foreach (var texture in MaskTextures)
            {
                texture?.Dispose();
            }

            // Should already be done, but just in case:
            foreach (var renderer in _tmpParams)
            {
                // List should be clear if this was disposed properly.
                renderer.Dispose();
            }

        }
    }
}
