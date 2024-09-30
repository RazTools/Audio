namespace Audio.Chunks.Types.HIRC;

public record RTPC : IBankReadable
{
    public RTPCEntry[] Entries { get; set; } = [];

    public void Read(BankReader reader)
    {
        short rtpcCount = reader.ReadInt16();
        Entries = new RTPCEntry[rtpcCount];
        for (int i = 0; i < rtpcCount; i++)
        {
            Entries[i] = new();
            Entries[i].Read(reader);
        }
    }
}