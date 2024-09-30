namespace Audio.Chunks.Types.HIRC;

public record RTPCGraphPoint : IBankReadable
{
    public float From { get; set; }
    public float To { get; set; }
    public CurveInterpolation Interp { get; set; }

    public void Read(BankReader reader)
    {
        From = reader.ReadSingle();
        To = reader.ReadSingle();
        Interp = (CurveInterpolation)reader.ReadUInt32();
    }
}