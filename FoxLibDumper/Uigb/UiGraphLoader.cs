using System.IO;

namespace FoxLibDumper.Uigb
{
    public static class UiGraphLoader
    {
        public static UiGraph ReadUiGraph(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                return UiGraph.Read(reader);
            }
        }
    }
}
