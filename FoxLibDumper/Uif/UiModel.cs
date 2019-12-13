using System;
using System.Collections.Generic;
using System.IO;

namespace FoxLibDumper.Uif
{
    public class UiModel
    {
        public IList<UiModelNodeElement> ModelNodeElements { get; } = new List<UiModelNodeElement>();
        public IList<uint> StrCode32Hashes { get; } = new List<uint>();
        public IList<ulong> PathFileNameCode64Hashes { get; } = new List<ulong>();

        public static UiModel Read(BinaryReader reader)
        {
            reader.BaseStream.Seek(10, SeekOrigin.Begin);
            var modelNodeCount = reader.ReadUInt16();
            var strCode32HashCount = reader.ReadUInt16();
            var fileReferenceCount = reader.ReadUInt16();

            reader.BaseStream.Seek(20, SeekOrigin.Begin);
            var strCode32HashesRelativeOffset = reader.ReadInt32();
            var fileHashesRelativeOffset = reader.ReadInt32();

            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            var hashesOffset = reader.ReadInt32();

            reader.BaseStream.Seek(hashesOffset + strCode32HashesRelativeOffset, SeekOrigin.Begin);

            var result = new UiModel();
            for (var i = 0; i < strCode32HashCount; i++)
            {
                result.StrCode32Hashes.Add(reader.ReadUInt32());
            }

            if (fileHashesRelativeOffset != -1)
            {
                reader.BaseStream.Seek(hashesOffset + fileHashesRelativeOffset, SeekOrigin.Begin);
                for (var i = 0; i < fileReferenceCount; i++)
                {
                    result.PathFileNameCode64Hashes.Add(reader.ReadUInt64());
                }
            }

            reader.BaseStream.Seek(56, SeekOrigin.Begin);
            for(var i = 0; i < modelNodeCount; i++)
            {
                var hashIndex = reader.ReadInt16();
                var type = reader.ReadByte();
                var node = new UiModelNodeElement(hashIndex == -1 ? null : new uint?(result.StrCode32Hashes[hashIndex]), (NodeType)type);
                result.ModelNodeElements.Add(node);

                reader.ReadBytes(5);
            }

            return result;
        }
    }
}
