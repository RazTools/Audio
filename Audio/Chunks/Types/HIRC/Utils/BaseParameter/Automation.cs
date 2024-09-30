namespace Audio.Chunks.Types.HIRC;

public record Automation : IBankReadable
{
    public float XRange { get; set; }
    public float YRange { get; set; }
    public float ZRange { get; set; }

    public void Read(BankReader reader)
    {
        XRange = reader.ReadSingle();
        YRange = reader.ReadSingle();
        ZRange = reader.ReadSingle();
    }
}