namespace Audio.Utils;
public class SegmentStream : Stream
{
    private readonly Stream _baseStream;
    private readonly long _basePosition;
    private readonly bool _leaveOpen;

    private long _position;
    private long _offset;
    private long _size;

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _size;
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public long Remaining => Length - Position;
    public long AbsolutePosition => _offset + _position;
    public long Offset
    {
        get => _offset;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _baseStream.Length);

            if (value + _size > _baseStream.Length)
                _size = _baseStream.Length - value;

            _offset = value;
            Position = 0;
        }
    }
    public long Size
    {
        get => _size;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value + _offset, _baseStream.Length);

            _size = value;
        }
    }

    public SegmentStream(Stream stream, bool leaveOpen = false) : this(stream, 0, stream.Length, leaveOpen) { }
    public SegmentStream(Stream stream, long offset, bool leaveOpen = false) : this(stream, offset, stream.Length - offset, leaveOpen) { }
    public SegmentStream(Stream stream, long offset, long size, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, stream.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + size, stream.Length);

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        }

        _size = size;
        _baseStream = stream;
        _leaveOpen = leaveOpen;
        _basePosition = stream.Position;
        
        Offset = offset;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length);

        int bytesToRead = (int)Math.Min(count, Remaining);
        if (bytesToRead == 0)
        {
            return 0;
        }
        else if (_position + bytesToRead > _size)
        {
            throw new IOException("Unable to read beyond the end of the stream.");
        }

        _baseStream.Position = _offset + _position;
        int bytesRead = _baseStream.Read(buffer, offset, bytesToRead);

        _position += bytesRead;
        return bytesRead;
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length);

        int bytesToWrite = (int)Math.Min(count, Remaining);
        if (bytesToWrite == 0)
        {
            return;
        }
        else if (_position + bytesToWrite > _size)
        {
            throw new IOException("Unable to write beyond the end of the stream.");
        }

        _baseStream.Position = _offset + _position;
        _baseStream.Write(buffer, offset, bytesToWrite);

        _position += bytesToWrite;
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        long position = origin switch
        {
            SeekOrigin.Begin => _offset + offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _offset + _size + offset,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin)),
        };

        if (position < _offset || position > _offset + _size)
        {
            throw new IOException("Invalid seek operation.");
        }

        return _position = position - _offset;
    }

    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (_baseStream.CanSeek)
        {
            _baseStream.Position = _basePosition;
        }

        if (!_leaveOpen)
        {
            _baseStream.Dispose();
        }
    }
}