namespace FoxLibLoaders
{
    using System;
    using System.IO;

    public static class FrldLoader
    {
        public static uint[] Read(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                Action<int> skipBytes = numberOfBytes => SkipBytes(reader, numberOfBytes);
                var readFunctions = new FoxLib.Tpp.RailUniqueIdFile.ReadFunctions(
                    reader.ReadUInt16, reader.ReadUInt32, skipBytes);
                return FoxLib.Tpp.RailUniqueIdFile.Read(readFunctions);
            }
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
    }
}
