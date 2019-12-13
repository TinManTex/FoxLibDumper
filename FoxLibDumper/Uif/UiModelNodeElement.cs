namespace FoxLibDumper.Uif
{
    public enum NodeType
    {
        Dummy = 0,
        Null = 1,
        Mesh = 2,
        Text = 3,
        Stencil = 4,
        Unknown = 5,
        Invalid = 6
    };

    public class UiModelNodeElement
    {
        public uint? NameHash { get; }
        public string Type => type.ToString();
        private NodeType type { get; }

        public UiModelNodeElement(uint? nameHash, NodeType type)
        {
            this.NameHash = nameHash;
            this.type = type;
        }
    }
}
