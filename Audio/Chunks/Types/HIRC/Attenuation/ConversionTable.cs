namespace Audio.Chunks.Types.HIRC;

public record ConversionTable : IBankReadable
{
    public CurveScaling Scaling { get; set; }
    public ushort Size { get; set; }
    public RTPCGraphPoint[] GraphPoints { get; set; } = [];

    public void Read(BankReader reader)
    {
        Scaling = (CurveScaling)reader.ReadByte();
        Size = reader.ReadUInt16();

        GraphPoints = new RTPCGraphPoint[Size];
        for (int i = 0; i < Size; i++)
        {
            GraphPoints[i] = new();
            GraphPoints[i].Read(reader);
        }
    }
}