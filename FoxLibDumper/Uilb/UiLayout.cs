using System.Collections.Generic;
using System.IO;

namespace FoxLibDumper.Uilb
{
    public class UiLayout
    {
        public IList<uint> StrCode32Hashes { get; } = new List<uint>();
        public IList<ulong> PathFileNameCode64Hashes { get; } = new List<ulong>();

        public static UiLayout Read(BinaryReader reader)
        {
            reader.BaseStream.Seek(16, SeekOrigin.Begin);
            var strCode32HashCount = reader.ReadUInt16();
            var fileReferenceCount = reader.ReadUInt16();

            reader.BaseStream.Seek(36, SeekOrigin.Begin);

            var strCode32HashesRelativeOffset = reader.ReadInt32();
            var fileHashesRelativeOffset = reader.ReadInt32();
            var hashesOffset = reader.ReadInt32();

            reader.BaseStream.Seek(hashesOffset + strCode32HashesRelativeOffset, SeekOrigin.Begin);

            var result = new UiLayout();
            for (var i = 0; i < strCode32HashCount; i++)
            {
                result.StrCode32Hashes.Add(reader.ReadUInt32());
            }

            reader.BaseStream.Seek(hashesOffset + fileHashesRelativeOffset, SeekOrigin.Begin);
            for (var i = 0; i < fileReferenceCount; i++)
            {
                result.PathFileNameCode64Hashes.Add(reader.ReadUInt64());
            }

            return result;
        }
    }
}
