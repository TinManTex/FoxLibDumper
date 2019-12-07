namespace FoxLibLoaders
{
    using System;
    using System.IO;

    public static class FormVariationLoader
    {
        public static FoxLib.FormVariation.FormVariation Read(string inputPath)
        {
            FoxLib.FormVariation.FormVariation formVariation = null;

            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                Action<int> skipBytes = numberOfBytes => SkipBytes(reader, numberOfBytes);
                Action<long> moveStream = bytePos => MoveStream(reader, bytePos);
                var readFunctions = new FoxLib.FormVariation.ReadFunctions(reader.ReadUInt16, reader.ReadUInt32, reader.ReadUInt64, reader.ReadByte, skipBytes, moveStream);
                try
                {
                    formVariation = FoxLib.FormVariation.Read(readFunctions);
                } catch
                {
                    //throw new Exception("Error: Unsupported .fv2 (it probably has to do with section 2)");
                    return null;
                }
            }

            return formVariation;
        }

        /// <summary>
        /// Skip reading a number of bytes.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <param name="numberOfBytes">The number of bytes to skip.</param>
        private static void SkipBytes(BinaryReader reader, long numberOfBytes)
        {
            reader.BaseStream.Position += numberOfBytes;
        }

        /// <summary>
        /// Move stream to a given position in bytes.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <param name="bytePos">The byte number to move the stream position to.</param>
        private static void MoveStream(BinaryReader reader, long bytePos)
        {
            reader.BaseStream.Position = bytePos;
        }
    }
}
