﻿using FFLSharp.Interop;
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
    class CharModelTexturesRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice; // Passed to DrawParamRenderer
        private readonly ICharModelResource _resourceManager;
        private readonly TextureManager _textureManager;

        private readonly ResourceFactory _factory; // Used for creating faceline/mask textures.

        // Faceline texture and framebuffer.
        public Texture? FacelineTexture;
        private readonly List<DrawParamGpuHandler> _tmpParams = new(); // Temporary DrawParamRenderer instances for texture drawing.
        // Mask textures and framebuffers, one for each expression.
        public readonly Texture[] MaskTextures = new Texture[(int)FFLExpression.FFL_EXPRESSION_MAX];

        // Framebuffers are disposed after drawing is finished.
        private readonly Framebuffer[] _maskFramebuffers = new Framebuffer[(int)FFLExpression.FFL_EXPRESSION_MAX];
        // ^^ Not all of these will be used or allocated to.
        private Framebuffer? _facelineFramebuffer;

        // CharModel field used for FFL calls such as FFLSetExpression.
        unsafe private readonly FFLCharModel* _pCharModel;

        public CharModelTexturesRenderer(GraphicsDevice graphicsDevice, ICharModelResource resourceManager,
            TextureManager textureManager, ResourceFactory factory, ref FFLCharModel charModel)
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
        }

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
                FacelineTexture = _factory.CreateTexture(
                    TextureDescription.Texture2D(
                        halfResolution, textureResolution, 1, 1, // Width, Height, MipLevels, SampleCount
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
                        textureResolution, textureResolution, 1, 1, // Width, Height, MipLevels, SampleCount
                        pixelFormat,                                // Desired pixel format
                        TextureUsage.RenderTarget | TextureUsage.Sampled)); // Need to render to this
                _maskFramebuffers[i] = _factory.CreateFramebuffer(
                    new FramebufferDescription(depthTarget: null, colorTargets: MaskTextures[i]));
            }

            /*
            int expression = (int)pModel->expression;
            FFLDrawParam* pMaskDrawParam = FFL.GetDrawParamXluMask((FFLCharModel*)Unsafe.AsPointer(ref charModel)); // usually returned as const
            var maskTextureHandle = _textureManager.AddTextureToMap(MaskTextures[expression]);
            pMaskDrawParam->modulateParam.pTexture2D = (void*)maskTextureHandle;
            */

            // Framebuffers are ready to bind and textures are ready to use.
        }

        private unsafe void DrawRenderTextures(ref FFLCharModel charModel, CommandList commandList, List<DrawParamGpuHandler> tmpParams)
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
            FFLColor facelineColor, CommandList commandList, List<DrawParamGpuHandler> tmpParams)
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
            CommandList commandList, List<DrawParamGpuHandler> tmpParams)
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

        private void DrawFromDrawParamOnce(ref FFLDrawParam param, CommandList commandList, List<DrawParamGpuHandler> tmpParams)
        {
            // Not meant for shapes because no view uniforms are ever set.
            Debug.Assert(param.modulateParam.type > FFLModulateType.FFL_MODULATE_TYPE_SHAPE_MAX - 1);
            // Create and use this DrawParamRenderer once.
            DrawParamGpuHandler renderer = new(_graphicsDevice, _resourceManager, _textureManager, ref param);
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
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, void* ptrMustNotBeNull, CommandList commandList, List<DrawParamGpuHandler> tmpParams)
        {
            if (ptrMustNotBeNull != null)
                DrawFromDrawParamOnce(ref param, commandList, tmpParams);
        }

        /// <summary>
        /// This overload additionally will only continue if mustNotBeZero is not zero (index count?).
        /// </summary>
        private unsafe void DrawFromDrawParamOnce(ref FFLDrawParam param, uint mustNotBeZero, CommandList commandList, List<DrawParamGpuHandler> tmpParams)
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