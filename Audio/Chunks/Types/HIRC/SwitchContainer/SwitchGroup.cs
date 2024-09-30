namespace Audio.Chunks.Types.HIRC;

public record SwitchGroup : IBankReadable
{
    public FNVID<uint> SwitchID { get; set; }
    public FNVID<uint>[] Nodes { get; set; } = [];

    public SwitchGroup()
    {
        SwitchID = 0;
    }

    public void Read(BankReader reader)
    {
        SwitchID = reader.ReadUInt32();

        int nodeCount = reader.ReadInt32();
        Nodes = new FNVID<uint>[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            Nodes[i] = reader.ReadUInt32();
        }
    }
}