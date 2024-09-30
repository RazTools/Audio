namespace Audio.Chunks.Types.HIRC;

public record TrackSrcInfo : IBankReadable
{
    public uint TrackID { get; set; }
    public FNVID<uint> SourceID { get; set; }
    public FNVID<uint> EventID { get; set; }
    public double PlayAt { get; set; }
    public double BeginTrimOffset { get; set; }
    public double EndTrimOffset { get; set; }
    public double SrcDuration { get; set; }

    public TrackSrcInfo()
    {
        SourceID = 0;
        EventID = 0;
    }

    public void Read(BankReader reader)
    {
        TrackID = reader.ReadUInt32();
        SourceID = reader.ReadUInt32();
        EventID = reader.ReadUInt32();
        PlayAt = reader.ReadDouble();
        BeginTrimOffset = reader.ReadDouble();
        EndTrimOffset = reader.ReadDouble();
        SrcDuration = reader.ReadDouble();
    }
}