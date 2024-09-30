namespace Audio.Chunks.Types.HIRC;

public record RandomizerModifier : IBankReadable
{
    public float Base { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }

    public void Read(BankReader reader)
    {
        Base = reader.ReadSingle();
        Min = reader.ReadSingle();
        Max = reader.ReadSingle();
    }
}