using Audio.Extensions;

namespace Audio;
public record HeaderInfo : IReadable<BinaryReader>
{
    public string Signature { get; set; } = "";
    public long Offset { get; set; }
    public uint Length { get; set; }

    public void Read(BinaryReader reader)
    {
        Signature = reader.ReadRawString(4);
        Length = reader.ReadUInt32();
        Offset = reader.BaseStream.Position;
    }

    public void Align(BinaryReader reader)
    {
        reader.BaseStream.Position += Length - (reader.BaseStream.Position - Offset);
    }
}