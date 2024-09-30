using Audio.Extensions;

namespace Audio.Chunks.Types.HIRC;

public record MusicMarker : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public double Position { get; set; }
    public string Name { get; set; } = "";

    public MusicMarker()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        Position = reader.ReadDouble();

        int nameSize = reader.ReadInt32();
        Name = reader.ReadRawString(nameSize);
    }
}