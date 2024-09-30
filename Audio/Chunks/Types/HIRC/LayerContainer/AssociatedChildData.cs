namespace Audio.Chunks.Types.HIRC;

public record AssociatedChildData : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public uint CurveSize { get; set; }
    public RTPCGraphPoint[] GraphPoints { get; set; } = [];

    public AssociatedChildData()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        CurveSize = reader.ReadUInt32();

        GraphPoints = new RTPCGraphPoint[CurveSize];
        for (int i = 0; i < CurveSize; i++)
        {
            GraphPoints[i] = new();
            GraphPoints[i].Read(reader);
        }
    }
}