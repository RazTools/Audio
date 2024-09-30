namespace Audio.Chunks.Types.HIRC;

public record Attenuation : HIRCObject
{
    public bool IsConeEnabled { get; set; }
    public Cone Cone { get; set; }
    public bool[] CurveToUse { get; set; }
    public ConversionTable[] Curves { get; set; } = [];
    public RTPC RTPC { get; set; }

    public Attenuation(HeaderInfo header) : base(header)
    {
        Cone = new();
        RTPC = new();
        CurveToUse = new bool[7];
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        IsConeEnabled = reader.ReadByte() != 0;

        if (IsConeEnabled)
        {
            Cone.Read(reader);
        }

        for (int i = 0; i < CurveToUse.Length; i++)
        {
            CurveToUse[i] = reader.ReadByte() != 0;
        }

        int curveCount = reader.ReadByte();
        Curves = new ConversionTable[curveCount];
        for (int i = 0; i < curveCount; i++)
        {
            Curves[i] = new();
            Curves[i].Read(reader);
        }

        RTPC.Read(reader);
    }
}