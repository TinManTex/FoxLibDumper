using System.IO;

namespace FoxLibDumper.Uif
{
    public static class UiModelLoader
    {
        public static UiModel ReadUiModel(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                return UiModel.Read(reader);
            }
        }
    }
}
