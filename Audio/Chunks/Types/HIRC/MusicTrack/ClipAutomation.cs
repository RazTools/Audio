namespace Audio.Chunks.Types.HIRC;

public record ClipAutomation : IBankReadable
{
    public uint ClipIndex { get; set; }
    public ClipAutomationType AutoType { get; set; }
    public uint PointsCount { get; set; }
    public RTPCGraphPoint[] GraphPoints { get; set; } = [];

    public void Read(BankReader reader)
    {
        ClipIndex = reader.ReadUInt32();
        AutoType = (ClipAutomationType)reader.ReadUInt32();

        PointsCount = reader.ReadUInt32();
        GraphPoints = new RTPCGraphPoint[PointsCount];
        for (int i = 0; i < GraphPoints.Length; i++)
        {
            GraphPoints[i] = new();
            GraphPoints[i].Read(reader);
        }
    }
}