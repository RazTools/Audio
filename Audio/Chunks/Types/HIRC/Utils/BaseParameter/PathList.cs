namespace Audio.Chunks.Types.HIRC;

public record PathList : IBankReadable
{
    public uint VerticesOffset { get; set; }
    public uint VerticesCount { get; set; }

    public void Read(BankReader reader)
    {
        VerticesOffset = reader.ReadUInt32();
        VerticesCount = reader.ReadUInt32();
    }
}