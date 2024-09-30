namespace Audio.Chunks.Types.HIRC;

public record ActorMixer : HIRCObject
{
    public BaseParameter Parameters { get; set; }
    public FNVID<uint>[] ChildrenIDs { get; set; } = [];
    public ActorMixer(HeaderInfo header) : base(header)
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
    }
}