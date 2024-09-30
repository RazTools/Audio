namespace Audio.Chunks.Types.HIRC;

public record RTPCEntry : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public StatePropertyID PropertyID { get; set; }
    public AccumulationType Type { get; set; }
    public StatePropertyID ParamID { get; set; }
    public FNVID<uint> CurveID { get; set; }
    public CurveScaling Scaling { get; set; }
    public ushort Size { get; set; }
    public RTPCGraphPoint[] GraphPoints { get; set; } = [];

    public RTPCEntry()
    {
        ID = 0;
        CurveID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        PropertyID = (StatePropertyID)reader.ReadByte();
        Type = (AccumulationType)reader.ReadByte();
        ParamID = (StatePropertyID)reader.Read7BitEncodedInt();
        CurveID = reader.ReadUInt32();
        Scaling = (CurveScaling)reader.ReadByte();
        Size = reader.ReadUInt16();

        GraphPoints = new RTPCGraphPoint[Size];
        for (int i = 0; i < GraphPoints.Length; i++)
        {
            GraphPoints[i] = new();
            GraphPoints[i].Read(reader);
        }
    }
}