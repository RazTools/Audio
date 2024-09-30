namespace Audio.Chunks.Types.HIRC;

public record MusicTransSrcRule : IBankReadable
{
    public int TransitionTime { get; set; }
    public CurveInterpolation FadeCurve { get; set; }
    public int FadeOffset { get; set; }
    public SyncType SyncType { get; set; }
    public uint CueFilterHash { get; set; }
    public byte PlayPostExit { get; set; }

    public void Read(BankReader reader)
    {
        TransitionTime = reader.ReadInt32();
        FadeCurve = (CurveInterpolation)reader.ReadUInt32();
        FadeOffset = reader.ReadInt32();
        SyncType = (SyncType)reader.ReadUInt32();
        CueFilterHash = reader.ReadUInt32();
        PlayPostExit = reader.ReadByte();
    }
}