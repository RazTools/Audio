namespace Audio.Chunks.Types.HIRC;

public record MeterInfo : IBankReadable
{
    public double GridPeriod { get; set; }
    public double GridOffset { get; set; }
    public float Tempo { get; set; }
    public byte TimeSigNumBeatsBar { get; set; }
    public byte TimeSigBeatValue { get; set; }

    public void Read(BankReader reader)
    {
        GridPeriod = reader.ReadDouble();
        GridOffset = reader.ReadDouble();
        Tempo = reader.ReadSingle();
        TimeSigNumBeatsBar = reader.ReadByte();
        TimeSigBeatValue = reader.ReadByte();
    }
}