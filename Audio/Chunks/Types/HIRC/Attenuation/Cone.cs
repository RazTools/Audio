namespace Audio.Chunks.Types.HIRC;

public record Cone : IBankReadable
{
    public float InsideDegrees { get; set; }
    public float OutsideDegrees { get; set; }
    public float OutsideVolume { get; set; }
    public float LoPass { get; set; }
    public float HiPass { get; set; }

    public void Read(BankReader reader)
    {
        InsideDegrees = reader.ReadSingle();
        OutsideDegrees = reader.ReadSingle();
        OutsideVolume = reader.ReadSingle();
        LoPass = reader.ReadSingle();
        HiPass = reader.ReadSingle();
    }
}