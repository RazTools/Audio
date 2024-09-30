namespace Audio.Chunks.Types.HIRC;

public record Event : HIRCObject
{
    public FNVID<uint>[] ActionsIDs { get; set; } = [];

    public Event(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        int actionCount = reader.Read7BitEncodedInt();
        ActionsIDs = new FNVID<uint>[actionCount];
        for (int i = 0; i < actionCount; i++)
        {
            ActionsIDs[i] = reader.ReadUInt32();
        }
    }
}