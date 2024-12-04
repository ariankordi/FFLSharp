﻿using FFLSharp.Interop; // For FFL.FFL_MIIDATA_PACKET_SIZE

namespace FFLSharp
{
    /// <summary>
    /// Represents samples of FFLStoreData instances occasionally used by me for testing.
    /// </summary>
    public static class SampleData
    {
        // Note: FFL.FFL_MIIDATA_PACKET_SIZE = 0x60 = 96 = sizeof(FFLStoreData)
        /// <summary>
        /// FFLStoreData representing the NNID JasmineChlora.
        /// Female, big eyes, faceMake, eyebrows, 4083 tri. hair, and glasses.
        /// </summary>
        public static readonly byte[] JasmineChlora = new byte[FFL.FFL_MIIDATA_PACKET_SIZE]
        {
            0x03, 0x00, 0x00, 0x40, 0xA0, 0x41, 0x38, 0xC4, 0xA0, 0x84, 0x00, 0x00, 0xDB, 0xB8, 0x87, 0x31,
            0xBE, 0x60, 0x2B, 0x2A, 0x2A, 0x42, 0x00, 0x00, 0x59, 0x2D, 0x4A, 0x00, 0x61, 0x00, 0x73, 0x00,
            0x6D, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x37,
            0x12, 0x10, 0x7B, 0x01, 0x21, 0x6E, 0x43, 0x1C, 0x0D, 0x64, 0xC7, 0x18, 0x00, 0x08, 0x1E, 0x82,
            0x0D, 0x00, 0x30, 0x41, 0xB3, 0x5B, 0x82, 0x6D, 0x00, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x69, 0x00,
            0x67, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x61, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x3A
        };
        /// <summary>
        /// FFLStoreData representing a character I call "bro" or "bro-mole-high".
        /// Male, glasses, mustache, mole, eyebrows, beard, cap, faceTex and faceMake.
        /// This character is seen in some essdeeekaayy docs and was recreated by
        /// eye and converted to Ver3 colors. The mole is slightly higher than in those docs.
        /// </summary>
        public static readonly byte[] BroMoleHigh = new byte[FFL.FFL_MIIDATA_PACKET_SIZE]
        {
            0x03, 0x00, 0x00, 0x40, 0xDF, 0x6E, 0x67, 0x47, 0xAA, 0xC6, 0x47, 0x34, 0xDB, 0x6B, 0xFB, 0x75,
            0xBC, 0xBC, 0xB0, 0x1B, 0xF8, 0xA2, 0x00, 0x00, 0x00, 0x04, 0x62, 0x00, 0x72, 0x00, 0x6F, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x40,
            0x2C, 0x92, 0x39, 0x02, 0x02, 0x89, 0x44, 0x16, 0x66, 0x34, 0x46, 0x10, 0xCD, 0x12, 0x0D, 0x48,
            0x4F, 0x00, 0xE2, 0x28, 0x22, 0x42, 0x89, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x4F
        };

        /// <summary>
        /// FFLStoreData representing Lane.
        /// Male, blond-ish hair, blond eyebrows, no faceline texture.
        /// </summary>
        public static readonly byte[] Lane = new byte[FFL.FFL_MIIDATA_PACKET_SIZE]
        {
            0x03, 0x01, 0x00, 0x30, 0x80, 0x21, 0x58, 0x64, 0x80, 0x44, 0x00, 0xA0, 0x91, 0xCD, 0xF6, 0x74, 0xE0, 0x0C, 0x7F, 0xE4, 0x7C, 0x69, 0x00, 0x00, 0x58, 0x58, 0x4C, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x00, 0x00, 0x74, 0x00, 0x00, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x7F, 0x2E, 0x08, 0x00, 0x33, 0x06, 0xA5, 0x28, 0x43, 0x12, 0xE1, 0x23, 0x84, 0x0E, 0x61, 0x10, 0x15, 0x86, 0x0D, 0x00, 0x20, 0x41, 0x00, 0x52, 0x10, 0x1D, 0x4C, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x91, 0x78
        };

        /// <summary>
        /// List of all of the sample data in this class.
        /// </summary>
        public static readonly List<byte[]> StoreDataSampleList = new()
        {
            JasmineChlora, BroMoleHigh, Lane
        };
    }
}