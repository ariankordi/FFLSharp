using System.Runtime.InteropServices;

namespace FFLSharp.Interop
{
    public static unsafe partial class FFL
    {
        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiVerifyCharInfo", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iVerifyCharInfo([NativeTypeName("const FFLiCharInfo *")] FFLiCharInfo* pCharInfo, [NativeTypeName("bool")] byte verifyName);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiVerifyCharInfoWithReason", ExactSpelling = true)]
        public static extern FFLiVerifyReason iVerifyCharInfoWithReason([NativeTypeName("const FFLiCharInfo *")] FFLiCharInfo* pCharInfo, [NativeTypeName("bool")] byte verifyName);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiiVerifyCharInfo", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iiVerifyCharInfo([NativeTypeName("const FFLiCharInfo *")] FFLiCharInfo* pCharInfo, [NativeTypeName("bool")] byte verifyName);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiiGetEyeRotateOffset", ExactSpelling = true)]
        [return: NativeTypeName("s32")]
        public static extern int iiGetEyeRotateOffset([NativeTypeName("s32")] int type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiiGetEyebrowRotateOffset", ExactSpelling = true)]
        [return: NativeTypeName("s32")]
        public static extern int iiGetEyebrowRotateOffset([NativeTypeName("s32")] int type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiiGetAdjustedMouthH", ExactSpelling = true)]
        [return: NativeTypeName("f32")]
        public static extern float iiGetAdjustedMouthH([NativeTypeName("f32")] float height, [NativeTypeName("s32")] int type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiiGetAdjustedEyeH", ExactSpelling = true)]
        [return: NativeTypeName("f32")]
        public static extern float iiGetAdjustedEyeH([NativeTypeName("f32")] float height, [NativeTypeName("s32")] int type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiIsValidCharacterForName", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iIsValidCharacterForName([NativeTypeName("u16")] ushort c);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitCharModelCPUStep", ExactSpelling = true)]
        public static extern FFLResult InitCharModelCPUStep(FFLCharModel* pModel, [NativeTypeName("const FFLCharModelSource *")] FFLCharModelSource* pSource, [NativeTypeName("const FFLCharModelDesc *")] FFLCharModelDesc* pDesc);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitCharModelCPUStepWithCallback", ExactSpelling = true)]
        public static extern FFLResult InitCharModelCPUStepWithCallback(FFLCharModel* pModel, [NativeTypeName("const FFLCharModelSource *")] FFLCharModelSource* pSource, [NativeTypeName("const FFLCharModelDesc *")] FFLCharModelDesc* pDesc, [NativeTypeName("const FFLTextureCallback *")] FFLTextureCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitCharModelGPUStep", ExactSpelling = true)]
        public static extern void InitCharModelGPUStep(FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitCharModelGPUStepWithCallback", ExactSpelling = true)]
        public static extern void InitCharModelGPUStepWithCallback(FFLCharModel* pModel, [NativeTypeName("const FFLShaderCallback *")] FFLShaderCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLDeleteCharModel", ExactSpelling = true)]
        public static extern void DeleteCharModel(FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaFaceline", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaFaceline([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaBeard", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaBeard([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaNose", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaNose([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaForehead", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaForehead([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaHair", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaHair([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamOpaCap", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamOpaCap([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLDrawOpa", ExactSpelling = true)]
        public static extern void DrawOpa([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLDrawOpaWithCallback", ExactSpelling = true)]
        public static extern void DrawOpaWithCallback([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel, [NativeTypeName("const FFLShaderCallback *")] FFLShaderCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamXluMask", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamXluMask([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamXluNoseLine", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamXluNoseLine([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetDrawParamXluGlass", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* GetDrawParamXluGlass([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLDrawXlu", ExactSpelling = true)]
        public static extern void DrawXlu([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLDrawXluWithCallback", ExactSpelling = true)]
        public static extern void DrawXluWithCallback([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel, [NativeTypeName("const FFLShaderCallback *")] FFLShaderCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetExpression", ExactSpelling = true)]
        public static extern void SetExpression(FFLCharModel* pModel, FFLExpression expression);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetExpression", ExactSpelling = true)]
        public static extern FFLExpression GetExpression([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLIsAvailableExpression", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte IsAvailableExpression([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel, FFLExpression expression);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetPartsTransform", ExactSpelling = true)]
        public static extern void GetPartsTransform(FFLPartsTransform* pTransform, [NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetViewModelType", ExactSpelling = true)]
        public static extern void SetViewModelType(FFLCharModel* pModel, FFLModelType type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetBoundingBox", ExactSpelling = true)]
        public static extern void GetBoundingBox(FFLBoundingBox* pBoundingBox, [NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetScale", ExactSpelling = true)]
        public static extern void SetScale([NativeTypeName("f32")] float scale);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetFaceTextureFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLiRenderTexture *")]
        public static extern FFLiRenderTexture* iGetFaceTextureFromCharModel([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetMaskTextureFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLiRenderTexture *")]
        public static extern FFLiRenderTexture* iGetMaskTextureFromCharModel([NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel, FFLExpression expression);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetCharInfoFromCharModel", ExactSpelling = true)]
        public static extern void iGetCharInfoFromCharModel(FFLiCharInfo* pCharInfo, [NativeTypeName("const FFLCharModel *")] FFLCharModel* pModel);

        [NativeTypeName("#define FFL_CHAR_MODEL_SIZE (int)sizeof(FFLiCharModel)")]
        public static readonly int FFL_CHAR_MODEL_SIZE = (int)(sizeof(FFLiCharModel));

        [NativeTypeName("#define FFL_CREATE_ID_SIZE (10)")]
        public const int FFL_CREATE_ID_SIZE = (10);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetRandomCharInfo", ExactSpelling = true)]
        public static extern void iGetRandomCharInfo(FFLiCharInfo* pCharInfo, FFLGender gender, FFLAge age, FFLRace race);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetAdditionalInfo", ExactSpelling = true)]
        public static extern FFLResult GetAdditionalInfo(FFLAdditionalInfo* pAdditionalInfo, FFLDataSource dataSource, [NativeTypeName("const void *")] void* pBuffer, [NativeTypeName("u16")] ushort index, [NativeTypeName("bool")] byte checkFontRegion);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiEnableSpecialMii", ExactSpelling = true)]
        public static extern void iEnableSpecialMii([NativeTypeName("u32")] uint key);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLpGetStoreDataFromCharInfo", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte pGetStoreDataFromCharInfo(FFLStoreData* pStoreData, [NativeTypeName("const FFLiCharInfo *")] FFLiCharInfo* pCharInfo);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLpGetCharInfoFromStoreData", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte pGetCharInfoFromStoreData(FFLiCharInfo* pCharInfo, FFLStoreData* pStoreData);

        [NativeTypeName("#define FFL_EXPRESSION_LIMIT 70")]
        public const int FFL_EXPRESSION_LIMIT = 70;

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitResEx", ExactSpelling = true)]
        public static extern FFLResult InitResEx([NativeTypeName("const FFLInitDesc *")] FFLInitDesc* pInitDesc, [NativeTypeName("const FFLResourceDesc *")] FFLResourceDesc* pResDesc);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitRes", ExactSpelling = true)]
        public static extern FFLResult InitRes(FFLFontRegion fontRegion, [NativeTypeName("const FFLResourceDesc *")] FFLResourceDesc* pResDesc);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitResGPUStep", ExactSpelling = true)]
        public static extern void InitResGPUStep();

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLFlushQuota", ExactSpelling = true)]
        public static extern FFLResult FlushQuota();

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLExit", ExactSpelling = true)]
        public static extern FFLResult Exit();

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLIsAvailable", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte IsAvailable();

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetFavoriteColor", ExactSpelling = true)]
        public static extern FFLColor GetFavoriteColor([NativeTypeName("s32")] int index);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetLinearGammaMode", ExactSpelling = true)]
        public static extern void SetLinearGammaMode([NativeTypeName("bool")] byte isLinear);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetFacelineColor", ExactSpelling = true)]
        public static extern FFLColor GetFacelineColor([NativeTypeName("s32")] int index);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetTextureFlipY", ExactSpelling = true)]
        public static extern void SetTextureFlipY([NativeTypeName("bool")] byte textureFlipY);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetNormalIsSnorm8_8_8_8", ExactSpelling = true)]
        public static extern void SetNormalIsSnorm8_8_8_8([NativeTypeName("bool")] byte enable);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetFrontCullForFlipX", ExactSpelling = true)]
        public static extern void SetFrontCullForFlipX([NativeTypeName("bool")] byte enable);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetExpressionFlagIndex", ExactSpelling = true)]
        public static extern void SetExpressionFlagIndex(FFLAllExpressionFlag* ef, [NativeTypeName("u32")] uint index, [NativeTypeName("bool")] byte set);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiIsHomeAuthorID", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iIsHomeAuthorID([NativeTypeName("const FFLiAuthorID *")] FFLiAuthorID* pAuthorID);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiIsSameAuthorID", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iIsSameAuthorID([NativeTypeName("const FFLiAuthorID *")] FFLiAuthorID* a, [NativeTypeName("const FFLiAuthorID *")] FFLiAuthorID* b);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiIsNullAuthorID", ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte iIsNullAuthorID([NativeTypeName("const FFLiAuthorID *")] FFLiAuthorID* pAuthorID);

        [NativeTypeName("#define FFLI_AUTHOR_ID_SIZE (8)")]
        public const int FFLI_AUTHOR_ID_SIZE = (8);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiInitCharModelCPUStep", ExactSpelling = true)]
        public static extern FFLResult iInitCharModelCPUStep(FFLiCharModel* pModel, [NativeTypeName("const FFLCharModelSource *")] FFLCharModelSource* pSource, [NativeTypeName("const FFLCharModelDesc *")] FFLCharModelDesc* pDesc, [NativeTypeName("const FFLTextureCallback *")] FFLTextureCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiInitCharModelGPUStep", ExactSpelling = true)]
        public static extern void iInitCharModelGPUStep(FFLiCharModel* pModel, [NativeTypeName("const FFLShaderCallback *")] FFLShaderCallback* pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDeleteCharModel", ExactSpelling = true)]
        public static extern void iDeleteCharModel(FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiSetExpression", ExactSpelling = true)]
        public static extern void iSetExpression(FFLiCharModel* pModel, FFLExpression expression);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetExpression", ExactSpelling = true)]
        public static extern FFLExpression iGetExpression([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetPartsTransform", ExactSpelling = true)]
        public static extern void iGetPartsTransform(FFLPartsTransform* pTransform, [NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiSetViewModelType", ExactSpelling = true)]
        public static extern void iSetViewModelType(FFLiCharModel* pModel, FFLModelType type);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetBoundingBoxCharModel", ExactSpelling = true)]
        public static extern void iGetBoundingBoxCharModel(FFLBoundingBox* pBoundingBox, [NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaFacelineFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaFacelineFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaBeardFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaBeardFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaNoseFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaNoseFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaForeheadFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaForeheadFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaHairFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaHairFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamOpaCapFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamOpaCapFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamXluMaskFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamXluMaskFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamXluNoseLineFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamXluNoseLineFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetDrawParamXluGlassFromCharModel", ExactSpelling = true)]
        [return: NativeTypeName("const FFLDrawParam *")]
        public static extern FFLDrawParam* iGetDrawParamXluGlassFromCharModel([NativeTypeName("const FFLiCharModel *")] FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiSetScale", ExactSpelling = true)]
        public static extern void iSetScale([NativeTypeName("f32")] float scale);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDeleteTempObjectFacelineTexture", ExactSpelling = true)]
        public static extern void iDeleteTempObjectFacelineTexture(FFLiFacelineTextureTempObject* pObject, [NativeTypeName("const FFLiCharInfo *")] FFLiCharInfo* pCharInfo, FFLResourceType resourceType);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiInvalidateTempObjectFacelineTexture", ExactSpelling = true)]
        public static extern void iInvalidateTempObjectFacelineTexture(FFLiFacelineTextureTempObject* pObject);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDrawFacelineTexture", ExactSpelling = true)]
        public static extern void iDrawFacelineTexture(FFLiFacelineTextureTempObject* pObject, FFLShaderCallback** pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDeleteTempObjectMaskTextures", ExactSpelling = true)]
        public static extern void iDeleteTempObjectMaskTextures(FFLiMaskTexturesTempObject* pObject, FFLAllExpressionFlag expressionFlag, FFLResourceType resourceType);

        [NativeTypeName("#define FFLI_MIDDLE_DB_PARAM_SIZE (4)")]
        public const int FFLI_MIDDLE_DB_PARAM_SIZE = (4);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiInvalidatePartsTextures", ExactSpelling = true)]
        public static extern void iInvalidatePartsTextures(FFLiPartsTextures* pPartsTextures);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiInvalidateRawMask", ExactSpelling = true)]
        public static extern void iInvalidateRawMask(FFLiRawMaskDrawParam* pDrawParam);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDrawRawMask", ExactSpelling = true)]
        public static extern void iDrawRawMask([NativeTypeName("const FFLiRawMaskDrawParam *")] FFLiRawMaskDrawParam* pDrawParam, FFLShaderCallback** pCallback);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiGetMaskMatrix", ExactSpelling = true)]
        public static extern void iGetMaskMatrix([NativeTypeName("float[16]")] float* pBaseMtx44f, [NativeTypeName("f32")] float resolution);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDeleteRenderTexture", ExactSpelling = true)]
        public static extern void iDeleteRenderTexture(FFLiRenderTexture* pRenderTexture);

        [NativeTypeName("#define FFLI_RENDER_TEXTURE_PLACEHOLDER (FFLiRenderTexture*)0x01")]
        public static readonly FFLiRenderTexture* FFLI_RENDER_TEXTURE_PLACEHOLDER = (FFLiRenderTexture*)(0x01);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLiDeleteTextureTempObject", ExactSpelling = true)]
        public static extern void iDeleteTextureTempObject(FFLiCharModel* pModel);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetMiddleDBBufferSize", ExactSpelling = true)]
        [return: NativeTypeName("u32")]
        public static extern uint GetMiddleDBBufferSize([NativeTypeName("u16")] ushort miiDataNum);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLInitMiddleDB", ExactSpelling = true)]
        public static extern void InitMiddleDB(FFLMiddleDB* pMiddleDB, FFLMiddleDBType type, void* pMiiData, [NativeTypeName("u16")] ushort miiDataNum);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLUpdateMiddleDB", ExactSpelling = true)]
        public static extern FFLResult UpdateMiddleDB(FFLMiddleDB* pMiddleDB);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetMiddleDBType", ExactSpelling = true)]
        public static extern FFLMiddleDBType GetMiddleDBType([NativeTypeName("const FFLMiddleDB *")] FFLMiddleDB* pMiddleDB);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetMiddleDBSize", ExactSpelling = true)]
        [return: NativeTypeName("s32")]
        public static extern int GetMiddleDBSize([NativeTypeName("const FFLMiddleDB *")] FFLMiddleDB* pMiddleDB);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetMiddleDBStoredSize", ExactSpelling = true)]
        [return: NativeTypeName("s32")]
        public static extern int GetMiddleDBStoredSize([NativeTypeName("const FFLMiddleDB *")] FFLMiddleDB* pMiddleDB);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetMiddleDBRandomMask", ExactSpelling = true)]
        public static extern void SetMiddleDBRandomMask(FFLMiddleDB* pMiddleDB, FFLGender gender, FFLAge age, FFLRace race);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetMiddleDBHiddenMask", ExactSpelling = true)]
        public static extern void SetMiddleDBHiddenMask(FFLMiddleDB* pMiddleDB, FFLGender gender);

        [NativeTypeName("#define FFL_MIDDLE_DB_SIZE (int)sizeof(FFLiMiddleDB)")]
        public static readonly int FFL_MIDDLE_DB_SIZE = (int)(sizeof(FFLiMiddleDB));

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLGetResourcePath", ExactSpelling = true)]
        public static extern FFLResult GetResourcePath([NativeTypeName("char *")] byte* pDst, [NativeTypeName("u32")] uint size, FFLResourceType resourceType, [NativeTypeName("bool")] byte LG);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetShaderCallback", ExactSpelling = true)]
        public static extern void SetShaderCallback([NativeTypeName("const FFLShaderCallback *")] FFLShaderCallback* pCallback);

        [NativeTypeName("#define FFL_MIIDATA_PACKET_SIZE (0x60)")]
        public const int FFL_MIIDATA_PACKET_SIZE = (0x60);

        [DllImport("ffl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "FFLSetTextureCallback", ExactSpelling = true)]
        public static extern void SetTextureCallback([NativeTypeName("const FFLTextureCallback *")] FFLTextureCallback* pCallback);

        [NativeTypeName("#define FFL_TEXTURE_PLACEHOLDER (FFLTexture*)0x01")]
        public static readonly void* FFL_TEXTURE_PLACEHOLDER = (void*)(0x01);
    }
}
