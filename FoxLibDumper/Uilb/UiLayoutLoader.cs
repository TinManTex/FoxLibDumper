using System.IO;

namespace FoxLibDumper.Uilb
{
    public static class UiLayoutLoader
    {
        public static UiLayout ReadUiLayout(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open)))
            {
                return UiLayout.Read(reader);
            }
        }
    }
}
