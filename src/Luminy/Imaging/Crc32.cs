namespace Luminy.Imaging
{
    using System;

    /// <summary>
    /// Computes standard IEEE 802.3 CRC-32 checksums for PNG chunks.
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] Table = GenerateTable();

        private static uint[] GenerateTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var c = i;
                for (var k = 0; k < 8; k++)
                {
                    c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                }
                table[i] = c;
            }
            return table;
        }

        /// <summary>
        /// Computes the CRC-32 checksum for a PNG chunk (evaluated over Type and Data).
        /// </summary>
        public static uint Compute(ReadOnlySpan<byte> chunkType, ReadOnlySpan<byte> chunkData)
        {
            var crc = 0xFFFFFFFFu;

            for (var i = 0; i < chunkType.Length; i++)
            {
                crc = Table[(crc ^ chunkType[i]) & 0xFF] ^ (crc >> 8);
            }

            for (var i = 0; i < chunkData.Length; i++)
            {
                crc = Table[(crc ^ chunkData[i]) & 0xFF] ^ (crc >> 8);
            }

            return crc ^ 0xFFFFFFFFu;
        }
    }
}
