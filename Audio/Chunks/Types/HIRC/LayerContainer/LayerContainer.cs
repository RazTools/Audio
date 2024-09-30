namespace Audio.Chunks.Types.HIRC;

public record LayerContainer : HIRCObject
{
    public BaseParameter Parameters { get; set; }
    public FNVID<uint>[] ChildrenIDs { get; set; } = [];
    public Layer[] Layers { get; set; } = [];
    public bool IsContinuousValidation { get; set; }

    public LayerContainer(HeaderInfo header) : base(header)
    {
        Parameters = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Parameters.Read(reader);

        int childrenCount = reader.ReadInt32();
        ChildrenIDs = new FNVID<uint>[childrenCount];
        for (int i = 0; i < childrenCount; i++)
        {
            ChildrenIDs[i] = reader.ReadUInt32();
        }

        int layerCount = reader.ReadInt32();
        Layers = new Layer[layerCount];
        for (int i = 0; i < layerCount; i++)
        {
            Layers[i] = new();
            Layers[i].Read(reader);
        }

        IsContinuousValidation = reader.ReadByte() != 0;
    }
}