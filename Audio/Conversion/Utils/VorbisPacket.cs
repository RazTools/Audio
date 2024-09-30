using System.Buffers.Binary;

namespace Audio.Conversion.Utils;
public record VorbisPacket
{
    private const int _headerSize = 2;
    private readonly long _offset;

    public long Offset => _offset + _headerSize;
    public long Next => Offset + Size;
    public ushort Size { get; private set; }

    public VorbisPacket(Stream stream, long offset)
    {
        _offset = offset;

        stream.Position = offset;

        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        Size = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }
}
