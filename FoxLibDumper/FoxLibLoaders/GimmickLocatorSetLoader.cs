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
    using static FoxLib.Tpp.GimmickLocatorSet.GimmickLocatorSet;
    using System.Linq;

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

        /// <summary>
        /// Read a .lba file and write the hashes of its locators to files.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        public static void ReadHashes(string filePath, ref Dictionary<string, HashSet<string>> hashes, ref List<string> failed)
        {
            var locatorSet = ReadLocatorSet(filePath);
            if (locatorSet == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            //DumpToJson(filePath, locatorSet);

            IEnumerable<Vector4> positions = null;

            // The locatorSet could be one of three different types.
            // Determine the type and cast it to get access to type-specific fields.
            if (locatorSet.IsPowerCutAreaGimmickLocatorSet)
            {
                var pca = locatorSet as PowerCutAreaGimmickLocatorSet;
                positions = from locator in pca.Locators
                            select locator.Position;
            } else if (locatorSet.IsNamedGimmickLocatorSet)
            {
                var named = locatorSet as NamedGimmickLocatorSet;
                positions = from locator in named.Locators
                            select locator.Position;

                foreach (var locator in named.Locators)
                {
                    hashes["DataSet"].Add(locator.DataSetName.ToString());
                    hashes["LocatorName"].Add(locator.LocatorName.ToString());
                }

            } else if (locatorSet.IsScaledGimmickLocatorSet)
            {
                var named = locatorSet as ScaledGimmickLocatorSet;
                positions = from locator in named.Locators
                            select locator.Position;

                foreach (var locator in named.Locators)
                {
                    hashes["DataSet"].Add(locator.DataSetName.ToString());
                    hashes["LocatorName"].Add(locator.LocatorName.ToString());
                }
            }
            foreach (var position in positions)
            {
                //tex OFF Console.WriteLine(position);
            }
        }//ReadHashes
    }
}
