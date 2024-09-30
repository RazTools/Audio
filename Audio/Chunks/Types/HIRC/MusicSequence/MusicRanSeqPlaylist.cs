namespace Audio.Chunks.Types.HIRC;

public record MusicRanSeqPlaylist : IBankReadable
{
    public FNVID<uint> SegmentID { get; set; }
    public FNVID<uint> PlaylistID { get; set; }
    public int ChildrenCount { get; set; }
    public RSType RSType { get; set; }
    public short Loop { get; set; }
    public short LoopMin { get; set; }
    public short LoopMax { get; set; }
    public uint Weight { get; set; }
    public ushort AvoidRepeatCount { get; set; }
    public bool IsUsingWeight { get; set; }
    public bool IsShuffle { get; set; }
    public MusicRanSeqPlaylist[] Children { get; set; } = [];

    public MusicRanSeqPlaylist()
    {
        SegmentID = 0;
        PlaylistID = 0;
    }

    public void Read(BankReader reader)
    {
        SegmentID = reader.ReadUInt32();
        PlaylistID = reader.ReadUInt32();
        ChildrenCount = reader.ReadInt32();
        RSType = (RSType)reader.ReadUInt32();
        Loop = reader.ReadInt16();
        LoopMin = reader.ReadInt16();
        LoopMax = reader.ReadInt16();
        Weight = reader.ReadUInt32();
        AvoidRepeatCount = reader.ReadUInt16();
        IsUsingWeight = reader.ReadByte() != 0;
        IsShuffle = reader.ReadByte() != 0;

        Children = new MusicRanSeqPlaylist[ChildrenCount];
        for (int i = 0; i < ChildrenCount; i++)
        {
            Children[i] = new();
            Children[i].Read(reader);
        }
    }
}