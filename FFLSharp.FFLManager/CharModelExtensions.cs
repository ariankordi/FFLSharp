using FFLSharp.Interop;
using System.Text; // UTF-16LE -> UTF-8 conversion

namespace FFLSharp
{
    /// <summary>
    /// Extension methods for FFLCharModel.
    /// </summary>
    public static class CharModelExtensions
    {
        /// <summary>
        /// Get FFLiCharInfo instance from a CharModel.
        /// </summary>
        public static unsafe FFLiCharInfo GetCharInfo(this FFLCharModel charModel)
        {
            return ((FFLiCharModel*)&charModel)->charInfo;
        }

        /// <summary>
        /// Get FFLiCharInfo pointer from a CharModel reference.
        /// This can potentially be used to change CharInfo
        /// before initialization, but after... loading it.
        /// </summary>
        public static unsafe FFLiCharInfo* GetpCharInfo(this ref FFLCharModel charModel)
        {
            // Cast FFLCharModel to FFLiCharModel and access charInfo.
            fixed (FFLCharModel* pCharModel = &charModel)
            {
                return &((FFLiCharModel*)pCharModel)->charInfo;
            }
        }

        public static unsafe FFLAdditionalInfo GetAdditionalInfo(this FFLCharModel charModel)
        {
            FFLAdditionalInfo additionalInfo = new FFLAdditionalInfo();
            // CharInfo will be our buffer.
            FFLiCharInfo* pCharInfo = &((FFLiCharModel*)&charModel)->charInfo;
            // Call FFLGetAdditionalInfo.
            FFLResult result = FFL.GetAdditionalInfo(&additionalInfo,
                FFLDataSource.FFL_DATA_SOURCE_BUFFER, pCharInfo, 0, 0); // No index and do not check FFLFontRegion
            // Check result and throw an exception if needed.
            FFLResultException.HandleResult(result);
            return additionalInfo; // Ready to use. No reference needed.
        }

        /// <summary>
        /// Get FFLPartsTransform instance from a CharModel.
        /// This structure gives you rotate and translate vectors
        /// for putting hats, headgear, accessories, etc. on the head.
        /// </summary>
        public static unsafe FFLPartsTransform GetPartsTransform(this FFLCharModel charModel)
        {
            // FFL.GetPartsTransform will work too.
            return ((FFLiCharModel*)&charModel)->partsTransform;
        }

        /// <summary>
        /// Method to verify if a certain expression was created
        /// and is available on a CharModel.
        /// </summary>
        public static unsafe bool IsAvailableExpression(this FFLCharModel charModel, FFLExpression expression)
        {
            // Effectively convert byte to bool.
            return FFL.IsAvailableExpression(&charModel, expression) == 1;
        }
    }
}
