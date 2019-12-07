namespace FoxLibLoaders
{
    using Vector4 = FoxLib.Core.Vector4;
    using Quaternion = FoxLib.Core.Quaternion;
    using WideVector3 = FoxLib.Core.WideVector3;
    using PowerCutAreaGimmickLocator = FoxLib.Tpp.GimmickLocator.PowerCutAreaGimmickLocator;
    using NamedGimmickLocator = FoxLib.Tpp.GimmickLocator.NamedGimmickLocator;
    using ScaledGimmickLocator = FoxLib.Tpp.GimmickLocator.ScaledGimmickLocator;

    using GimmickLocatorSet = FoxLib.Tpp.GimmickLocatorSet.GimmickLocatorSet;

    using System.Collections.Generic;
    using System;
    using System.IO;

    /// <summary>
    /// Examples of working with GimmickLocatorSets (.lba files).
    /// </summary>
    public static class GimmickLocatorSetLoader
    {
        /// <summary>
        /// Reads a .lba file and parses it into a GimmickLocatorSet.
        /// </summary>
        /// <param name="inputPath">File to read.</param>
        /// <returns>The parsed locatorSet.</returns>
        public static GimmickLocatorSet ReadLocatorSet(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                Action<int> skipBytes = numberOfBytes => SkipBytes(reader, numberOfBytes);
                var readFunctions = new FoxLib.Tpp.GimmickLocatorSet.ReadFunctions(
                    reader.ReadSingle, reader.ReadUInt16, reader.ReadUInt32, reader.ReadInt32, skipBytes);
                return FoxLib.Tpp.GimmickLocatorSet.Read(readFunctions);
            }
        }

        /// <summary>
        /// Skip reading a number of bytes.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <param name="numberOfBytes">The number of bytes to skip.</param>
        private static void SkipBytes(BinaryReader reader, int numberOfBytes)
        {
            reader.BaseStream.Position += numberOfBytes;
        }
    }
}
