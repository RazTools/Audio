namespace Audio.Chunks.Types.HIRC;

public record PathVertex : IBankReadable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public int Duration { get; set; }

    public void Read(BankReader reader)
    {
        X = reader.ReadSingle();
        Y = reader.ReadSingle();
        Z = reader.ReadSingle();
        Duration = reader.ReadInt32();
    }
}