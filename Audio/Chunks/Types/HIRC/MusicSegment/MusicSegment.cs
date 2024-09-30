namespace Audio.Chunks.Types.HIRC;

public record MusicSegment : HIRCObject
{
    public MusicParameter Parameters { get; set; }
    public double Duration { get; set; }
    public MusicMarker[] Markers { get; set; } = [];

    public MusicSegment(HeaderInfo header) : base(header)
    {
        Parameters = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Parameters.Read(reader);
        Duration = reader.ReadDouble();

        int markerCount = reader.ReadInt32();
        Markers = new MusicMarker[markerCount];
        for (int i = 0; i < markerCount; i++)
        {
            Markers[i] = new();
            Markers[i].Read(reader);
        }
    }
}