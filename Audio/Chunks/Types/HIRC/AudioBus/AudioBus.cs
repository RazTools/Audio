namespace Audio.Chunks.Types.HIRC;

public record AudioBus : HIRCObject
{
    public FNVID<uint> OverrideBusId { get; set; }
    public FNVID<uint> IDDeviceShareSet { get; set; }
    public Bus Bus { get; set; }
    public int RecoveryTime { get; set; }
    public float MaxDuckVolume { get; set; }
    public DuckInfo[] Ducks { get; set; } = [];
    public BusEffect Effect { get; set; }
    public byte OverrideAttachmentParams { get; set; }
    public RTPC RTPC { get; set; }
    public StateChunk StateChunk { get; set; }

    public AudioBus(HeaderInfo header) : base(header)
    {
        OverrideBusId = 0;
        IDDeviceShareSet = 0;
        Bus = new();
        Effect = new();
        RTPC = new();
        StateChunk = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        OverrideBusId = reader.ReadUInt32();
        if (OverrideBusId == 0)
        {
            IDDeviceShareSet = reader.ReadUInt32();
        }

        Bus.Read(reader);
        RecoveryTime = reader.ReadInt32();
        MaxDuckVolume = reader.ReadSingle();

        int duckCount = reader.ReadInt32();
        Ducks = new DuckInfo[duckCount];
        for (int i = 0; i < duckCount; i++)
        {
            Ducks[i] = new();
            Ducks[i].Read(reader);
        }

        Effect.Read(reader);
        OverrideAttachmentParams = reader.ReadByte();
        RTPC.Read(reader);
        StateChunk.Read(reader);
    }
}