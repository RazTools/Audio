using Audio.Extensions;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using static Audio.Conversion.Utils.BitHelper;

namespace Audio.Conversion.Utils;
public class OGGStream : BitStream
{
    private const string Header = "OggS";
    private const int MaxSegments = 0xFF;
    private const int SegmentSize = 0xFF;
    private const int HeaderSize = 0x1B;
    private const int SerialNumber = 1;
    private const int Version = 0;

    private static int MaxSegmentSize => SegmentSize * MaxSegments * BitsInByte;
    private static int MaxSegmentOffset => (HeaderSize + MaxSegments) * BitsInByte;

    private readonly Stream _outstream;
    private uint _sequanceNumber;

    public OGGStream(Stream stream) : base(new MemoryStream())
    {
        _outstream = stream ?? throw new ArgumentNullException(nameof(stream));

        SetLength(MaxSegmentOffset + MaxSegmentSize);

        Position = MaxSegmentOffset;
        Type = PageType.Head;
        BufferSize = 0x1000;
    }

    public long Granule { get; set; }
    public long BufferSize { get; set; }
    public PageType Type { get; set; }

    private List<long> Packets { get; init; } = [];
    private long PageDataSize => (Position - MaxSegmentOffset) / BitsInByte;

    public override MemoryStream BaseStream => (MemoryStream)base.BaseStream;
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override void Flush()
    {
        if (PageDataSize == SegmentSize * MaxSegments)
        {
            Logger.Warning("Exceeded maximum allowed page size, flushing...");
            FlushPage();
            Type |= PageType.Continued;
        }

        base.Flush();
    }

    public void FlushPacket(bool allowPage = false)
    {
        if (PageDataSize != SegmentSize * MaxSegments)
        {
            Flush();
        }

        Packets.Add(PageDataSize - Packets.Sum());

        if (allowPage && (Packets.Count == MaxSegments || Packets.Sum() >= BufferSize))
        {
            FlushPage();
        }
    }

    public void FlushPage()
    {
        if (PageDataSize != SegmentSize * MaxSegments)
        {
            Flush();
        }

        if (Packets.Count == 0)
        {
            Packets.Add(PageDataSize);
        }

        if (PageDataSize != 0)
        {
            int segmentCount = 0;
            foreach (long size in Packets)
            {
                long remaining = size;
                do
                {
                    segmentCount++;
                    remaining -= SegmentSize;
                } while (remaining >= 0);
            }

            segmentCount = Math.Clamp(segmentCount, 0, MaxSegments);

            byte[] pageBuffer = BaseStream.GetBuffer();

            int pageSize = HeaderSize + segmentCount + (int)PageDataSize;
            Buffer.BlockCopy(pageBuffer, HeaderSize + MaxSegments, pageBuffer, HeaderSize + segmentCount, pageSize);

            Encoding.UTF8.GetBytes(Header).CopyTo(pageBuffer.AsSpan());
            pageBuffer[4] = Version;
            pageBuffer[5] = (byte)Type;
            BinaryPrimitives.WriteInt64LittleEndian(pageBuffer.AsSpan(6), Granule);
            BinaryPrimitives.WriteUInt32LittleEndian(pageBuffer.AsSpan(14), SerialNumber);
            BinaryPrimitives.WriteUInt32LittleEndian(pageBuffer.AsSpan(18), _sequanceNumber);
            BinaryPrimitives.WriteUInt32LittleEndian(pageBuffer.AsSpan(22), 0); // checksum
            pageBuffer[26] = (byte)segmentCount;

            int i = 0;
            foreach (long size in Packets)
            {
                long remaining = size;

                do
                {
                    pageBuffer[HeaderSize + i++] = (byte)(remaining >= SegmentSize ? SegmentSize : remaining);
                    remaining -= SegmentSize;
                } while (remaining >= 0);
            }

            uint checksum = CRC.CalculateDigest(pageBuffer.AsSpan(0, pageSize));
            BinaryPrimitives.WriteUInt32LittleEndian(pageBuffer.AsSpan(22), checksum);

            BaseStream.Position = 0;
            BaseStream.CopyTo(_outstream, (long)pageSize);

            _outstream.Flush();

            _sequanceNumber++;
            Type = PageType.None;
            Position = MaxSegmentOffset;
            Packets.Clear();
        }
    }

    protected override void Dispose(bool disposing)
    {
        FlushPage();
        base.Dispose(disposing);
    }

    [Flags]
    public enum PageType
    {
        None = 0,
        Continued = 1 << 0,
        Head = 1 << 1,
        Tail = 1 << 2
    }
}