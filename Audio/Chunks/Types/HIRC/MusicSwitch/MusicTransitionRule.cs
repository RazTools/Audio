namespace Audio.Chunks.Types.HIRC;

public record MusicTransitionRule : IBankReadable
{
    public FNVID<uint>[] Sources { get; set; } = [];
    public FNVID<uint>[] Destinations { get; set; } = [];
    public MusicTransSrcRule SourceTransitionRule { get; set; }
    public MusicTransDstRule DestinationTransitionRule { get; set; }
    public byte AllocTransObjectFlag { get; set; }
    public MusicTransitionObject TransitionObject { get; set; }

    public MusicTransitionRule()
    {
        SourceTransitionRule = new();
        DestinationTransitionRule = new();
        TransitionObject = new();
    }

    public void Read(BankReader reader)
    {
        int sourceCount = reader.ReadInt32();
        Sources = new FNVID<uint>[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            Sources[i] = reader.ReadUInt32();
        }

        int destinationCount = reader.ReadInt32();
        Destinations = new FNVID<uint>[destinationCount];
        for (int i = 0; i < destinationCount; i++)
        {
            Destinations[i] = reader.ReadUInt32();
        }

        SourceTransitionRule.Read(reader);
        DestinationTransitionRule.Read(reader);
        AllocTransObjectFlag = reader.ReadByte();

        if (AllocTransObjectFlag != 0)
        {
            TransitionObject.Read(reader);
        }
    }
}