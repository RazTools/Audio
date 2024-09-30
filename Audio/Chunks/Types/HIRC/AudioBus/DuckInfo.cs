namespace Audio.Chunks.Types.HIRC;

public record DuckInfo : IBankReadable
{
    public FNVID<uint> BusID { get; set; }
    public float DuckVolume { get; set; }
    public int FadeOutTime { get; set; }
    public int FadeInTime { get; set; }
    public CurveInterpolation FadeCurve { get; set; }
    public PropertyID TargetProp { get; set; }

    public DuckInfo()
    {
        BusID = 0;
    }

    public void Read(BankReader reader)
    {
        BusID = reader.ReadUInt32();
        DuckVolume = reader.ReadSingle();
        FadeOutTime = reader.ReadInt32();
        FadeInTime = reader.ReadInt32();
        FadeCurve = (CurveInterpolation)reader.ReadByte();
        TargetProp = (PropertyID)reader.ReadByte();
    }
}