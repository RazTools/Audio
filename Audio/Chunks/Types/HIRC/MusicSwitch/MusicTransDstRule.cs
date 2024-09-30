namespace Audio.Chunks.Types.HIRC;

public record MusicTransDstRule : IBankReadable
{
    public int TransitionTime { get; set; }
    public CurveInterpolation FadeCurve { get; set; }
    public int FadeOffset { get; set; }
    public uint CueFilterHash { get; set; }
    public FNVID<uint> JumpToID { get; set; }
    public JumpToSelType JumpToType { get; set; }
    public TransitionEntryType EntryType { get; set; }
    public byte PlayPreEntry { get; set; }
    public byte DestMatchSourceCueName { get; set; }

    public MusicTransDstRule()
    {
        JumpToID = 0;
    }

    public void Read(BankReader reader)
    {
        TransitionTime = reader.ReadInt32();
        FadeCurve = (CurveInterpolation)reader.ReadUInt32();
        FadeOffset = reader.ReadInt32();
        CueFilterHash = reader.ReadUInt32();
        JumpToID = reader.ReadUInt32();
        JumpToType = (JumpToSelType)reader.ReadUInt16();
        EntryType = (TransitionEntryType)reader.ReadUInt16();
        PlayPreEntry = reader.ReadByte();
        DestMatchSourceCueName = reader.ReadByte();
    }
}