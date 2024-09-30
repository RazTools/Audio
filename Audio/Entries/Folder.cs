using Audio.Extensions;
using System.Text;

namespace Audio.Entries;
public record Folder : IReadable<BinaryReader>
{
    private readonly long _baseOffset;

    public FNVID<uint> ID { get; set; }
    public uint Offset { get; set; }
    public string? Name { get; set; }

    public Folder(long baseOffset)
    {
        _baseOffset = baseOffset;
    }

    public void Read(BinaryReader reader)
    {
        Offset = reader.ReadUInt32();
        ID = reader.ReadUInt32();

        long curOffset = reader.BaseStream.Position;

        reader.BaseStream.Seek(_baseOffset + Offset, SeekOrigin.Begin);

        bool isUTF16 = true;
        StringBuilder sb = new();
        while (reader.PeekChar() != '\0')
        {
            sb.Append(reader.ReadChar());
            if (isUTF16 && reader.PeekChar() != '\0')
            {
                isUTF16 = false;
            }

            if (isUTF16)
            {
                reader.BaseStream.Position++;
            }
        }

        Name = sb.ToString();
        FNVID<uint>.TryMatch(Name, out _);

        reader.BaseStream.Position = curOffset;
    }
}
