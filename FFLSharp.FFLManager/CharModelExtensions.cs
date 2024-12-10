using FFLSharp.Interop;
using System;
using System.Collections.Generic;
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

        public const int NameLengthBytes = 10; // names in CharInfo are always 20 bytes
                                               // with extra padding makes them 22 for null


        public static unsafe string ConvertUtf16PointerToUtf8String(ushort* utf16Pointer, int length)
        {
            if (utf16Pointer == null)
            {
                throw new ArgumentNullException(nameof(utf16Pointer));
            }

            if (length <= 0)
            {
                throw new ArgumentException("Length must be greater than zero.", nameof(length));
            }

            // Create a list of bytes for the UTF-16LE data
            List<byte> utf16Bytes = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                if (utf16Pointer[i] == 0)
                {
                    break; // Hit a null terminator, stop processing.
                }
                // Process UTF-16LE.
                utf16Bytes.Add((byte)(utf16Pointer[i] & 0xFF));        // Lower byte
                utf16Bytes.Add((byte)((utf16Pointer[i] >> 8) & 0xFF)); // Upper byte
            }

            // Decode UTF-16LE bytes to a .NET string
            string utf16String = Encoding.Unicode.GetString(utf16Bytes.ToArray());

            // Convert the .NET string to UTF-8
            return utf16String;
        }

        public static unsafe string GetName(this FFLCharModel charModel)
        {
            ushort* pName = ((FFLiCharModel*)&charModel)->charInfo.name;
            return ConvertUtf16PointerToUtf8String(pName, NameLengthBytes);
        }
        public static unsafe string GetCreatorName(this FFLCharModel charModel)
        {
            ushort* pName = ((FFLiCharModel*)&charModel)->charInfo.creatorName;
            return ConvertUtf16PointerToUtf8String(pName, NameLengthBytes);
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
