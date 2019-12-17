namespace FoxLibLoaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FormVariation = FoxLib.FormVariation.FormVariation;

    public static class FormVariationLoader
    {
        public static FormVariation Read(string inputPath)
        {
            FormVariation formVariation = null;

            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                Action<int> skipBytes = numberOfBytes => SkipBytes(reader, numberOfBytes);
                Action<long> moveStream = bytePos => MoveStream(reader, bytePos);
                var readFunctions = new FoxLib.FormVariation.ReadFunctions(reader.ReadUInt16, reader.ReadUInt32, reader.ReadUInt64, reader.ReadByte, skipBytes, moveStream);
                try
                {
                    formVariation = FoxLib.FormVariation.Read(readFunctions);
                }
                catch (Exception e)
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

        public static void ReadHashes(string filePath, ref Dictionary<string, HashSet<string>> hashes, ref List<string> failed)
        {
            FormVariation formVariation = Read(filePath);
            if (formVariation == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            //DumpToJson(filePath, formVariation);

            foreach (var hash in formVariation.HiddenMeshGroups)
            {
                //hashes["HiddenMeshGroup"].Add(hash.ToString());//s32
                hashes["MeshGroup"].Add(hash.ToString());//s32
            }
            foreach (var hash in formVariation.ShownMeshGroups)
            {
                //hashes["ShownMeshGroup"].Add(hash.ToString());//s32
                hashes["MeshGroup"].Add(hash.ToString());//s32
            }
            foreach (var textureSwap in formVariation.TextureSwaps)
            {
                hashes["MaterialInstance"].Add(textureSwap.MaterialInstanceHash.ToString());//s32
                hashes["TextureType"].Add(textureSwap.TextureTypeHash.ToString());//s32
                hashes["TexturePath"].Add(textureSwap.TextureFileHash.ToString());//p64
            }
            foreach (var boneAttachment in formVariation.BoneAttachments)
            {
                hashes["ModelPath"].Add(boneAttachment.ModelFileHash.ToString());//p64
                hashes["FrdvPath"].Add(boneAttachment.FrdvFileHash.ToString());//p64
                hashes["SimPath"].Add(boneAttachment.SimFileHash.ToString());//p64
            }
            foreach (var cnpAttachment in formVariation.CNPAttachments)
            {
                hashes["ConnectionPoint"].Add(cnpAttachment.CNPHash.ToString());//s32
                hashes["ModelPath"].Add(cnpAttachment.ModelFileHash.ToString());//p64
                hashes["FrdvPath"].Add(cnpAttachment.FrdvFileHash.ToString());//p64
                hashes["SimPath"].Add(cnpAttachment.SimFileHash.ToString());//p64
            }

            foreach (var variableEntry in formVariation.Variables)
            {
                foreach (var subEntry in variableEntry)
                {
                    foreach (var mesh in subEntry.Meshes)
                    {
                        hashes["MeshName"].Add(mesh.ToString());//s32
                    }

                    foreach (var textureSwap in subEntry.MaterialInstances)
                    {
                        hashes["MaterialInstance"].Add(textureSwap.MaterialInstanceHash.ToString());//s32
                        hashes["TextureType"].Add(textureSwap.TextureTypeHash.ToString());//s32
                        hashes["TexturePath"].Add(textureSwap.TextureFileHash.ToString());//p64
                    }

                    foreach (var boneAttachment in subEntry.BoneAttachments)
                    {
                        hashes["ModelPath"].Add(boneAttachment.ModelFileHash.ToString());
                        hashes["FrdvPath"].Add(boneAttachment.FrdvFileHash.ToString());
                        hashes["SimPath"].Add(boneAttachment.SimFileHash.ToString());
                    }

                    foreach (var cnp in subEntry.CNPAttachments)
                    {
                        hashes["ConnectionPoint"].Add(cnp.CNPHash.ToString());//s32
                        hashes["ModelPath"].Add(cnp.ModelFileHash.ToString());//p64
                        hashes["FrdvPath"].Add(cnp.FrdvFileHash.ToString());//p64
                        hashes["SimPath"].Add(cnp.SimFileHash.ToString());//p64
                    }
                }
            }//foreach variableEntry
        }//ReadHashes
    }//FormVariationLoader
}
