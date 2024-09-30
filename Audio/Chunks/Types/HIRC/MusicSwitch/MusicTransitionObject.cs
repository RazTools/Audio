namespace Audio.Chunks.Types.HIRC;

public record MusicTransitionObject : IBankReadable
{
    public FNVID<uint> SegmentID { get; set; }
    public MusicFade FadeIn { get; set; }
    public MusicFade FadeOut { get; set; }
    public byte PlayPreEntry { get; set; }
    public byte PlayPostExit { get; set; }

    public MusicTransitionObject()
    {
        SegmentID = 0;
        FadeIn = new();
        FadeOut = new();
    }

    public void Read(BankReader reader)
    {
        SegmentID = reader.ReadUInt32();
        FadeIn.Read(reader);
        FadeOut.Read(reader);
        PlayPreEntry = reader.ReadByte();
        PlayPostExit = reader.ReadByte();
    }
}