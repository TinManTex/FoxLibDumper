using System.Collections.Generic;
using System.IO;

namespace FoxLibDumper.Uigb
{
    public class UiGraph
    {
        public IList<UiGraphNode> Nodes { get; } = new List<UiGraphNode>();
        public IList<uint> StrCode32Hashes { get; } = new List<uint>();
        public IList<ulong> PathFileNameCode64Hashes { get; } = new List<ulong>();

        public static UiGraph Read(BinaryReader reader)
        {
            reader.BaseStream.Seek(14, SeekOrigin.Begin);

            var fileReferenceCount = reader.ReadUInt16();
            var strCode32HashCount = reader.ReadUInt32();

            reader.BaseStream.Seek(40, SeekOrigin.Begin);
            var filePathHashOffset = reader.ReadInt32();
            var hashTableOffset = reader.ReadInt32();

            reader.BaseStream.Seek(52, SeekOrigin.Begin);
            var section5Offset = reader.ReadInt32();

            reader.BaseStream.Seek(section5Offset + hashTableOffset, SeekOrigin.Begin);

            var result = new UiGraph();
            for(var i = 0; i < strCode32HashCount; i++)
            {
                result.StrCode32Hashes.Add(reader.ReadUInt32());
            }

            reader.BaseStream.Seek(section5Offset + filePathHashOffset, SeekOrigin.Begin);
            for (var i = 0; i < fileReferenceCount; i++)
            {
                result.PathFileNameCode64Hashes.Add(reader.ReadUInt64());
            }

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
            var nodeCount = reader.ReadUInt16();

            reader.BaseStream.Seek(56, SeekOrigin.Begin);
            for (var i = 0; i < nodeCount; i++)
            {
                var startPosition = reader.BaseStream.Position;

                byte nodeSizeInBytes;
                var node = UiGraphNode.Read(reader, index => result.StrCode32Hashes[index], out nodeSizeInBytes);
                result.Nodes.Add(node);

                reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
                reader.BaseStream.Seek(startPosition + nodeSizeInBytes, SeekOrigin.Begin);
            }

            return result;
        }
    }
}
