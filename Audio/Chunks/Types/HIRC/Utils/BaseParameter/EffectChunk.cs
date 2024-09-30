namespace Audio.Chunks.Types.HIRC;

public record EffectChunk : IBankReadable
{
    public byte Index { get; set; }
    public FNVID<uint> ID { get; set; }
    public bool IsShareSet { get; set; }
    public bool IsRendered { get; set; }

    public EffectChunk()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        Index = reader.ReadByte();
        ID = reader.ReadUInt32();
        IsShareSet = reader.ReadByte() != 0;
        IsRendered = reader.ReadByte() != 0;
    }
}