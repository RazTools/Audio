namespace Audio.Chunks.Types.HIRC;

public record Playlist : IBankReadable
{
    public FNVID<uint> PlayID { get; set; }
    public int Weight { get; set; }

    public Playlist()
    {
        PlayID = 0;
    }

    public void Read(BankReader reader)
    {
        PlayID = reader.ReadUInt32();
        Weight = reader.ReadInt32();
    }
}