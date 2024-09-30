namespace Audio.Chunks.Types.HIRC;

public record Stinger : IBankReadable
{
    public FNVID<uint> TriggerID { get; set; }
    public FNVID<uint> SegmentID { get; set; }
    public SyncType SyncPlayAt { get; set; }
    public uint CueFilterHash { get; set; }
    public int DontRepeatTime { get; set; }
    public uint SegmentCountLookAhead { get; set; }

    public Stinger()
    {
        TriggerID = 0;
        SegmentID = 0;
    }

    public void Read(BankReader reader)
    {
        TriggerID = reader.ReadUInt32();
        SegmentID = reader.ReadUInt32();
        SyncPlayAt = (SyncType)reader.ReadUInt32();
        CueFilterHash = reader.ReadUInt32();
        DontRepeatTime = reader.ReadInt32();
        SegmentCountLookAhead = reader.ReadUInt32();
    }
}