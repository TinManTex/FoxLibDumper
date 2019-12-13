using System;
using System.IO;

namespace FoxLibDumper.Uigb
{
    public enum UiNodeType
    {
        NODE_PAGE = 0,
        NODE_PHASE = 1,
        NODE_EVENT = 2,
        NODE_ACTION = 3,
        NODE_OPERATION = 4,
        NODE_UNKNOWN = 5,
        NODE_COMPOUND = 6
    };

    public class UiGraphNode
    {
        public uint TypeHash { get; }
        public uint NameHash { get; }
        public string Type => type.ToString();

        private UiNodeType type { get; }

        public UiGraphNode(uint typeHash, uint nameHash, UiNodeType type)
        {
            this.TypeHash = typeHash;
            this.NameHash = nameHash;
            this.type = type;
        }

        public static UiGraphNode Read(BinaryReader reader, Func<int, uint> getStrCode32HashByIndex, out byte nodeSizeInBytes)
        {
            var nodeTypeHashIndex = reader.ReadUInt16();
            var nodeNameHashIndex = reader.ReadUInt16();

            nodeSizeInBytes = reader.ReadByte();

            var type = reader.ReadByte();

            return new UiGraphNode(getStrCode32HashByIndex(nodeTypeHashIndex),
                getStrCode32HashByIndex(nodeNameHashIndex),
                (UiNodeType)type);
        }
    }
}
