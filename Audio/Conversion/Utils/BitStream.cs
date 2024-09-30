using static Audio.Conversion.Utils.BitHelper;

namespace Audio.Conversion.Utils;
public class BitStream : Stream
{
    private readonly Stream _baseStream;
    private readonly bool _leaveOpen;
    private readonly bool _writable;
    private readonly byte[] _buffer;

    private int _position;
    private int _index;

    public BitStream(Stream stream, bool writable = true, bool leaveOpen = false)
    {
        _baseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
        _writable = writable;

        _buffer = new byte[1];
        _position = 0;
        _index = 0;
    }

    public virtual Stream BaseStream => _baseStream;
    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _writable;
    public override long Length => _baseStream.Length * BitsInByte;
    public override long Position
    {
        get => _position * BitsInByte + _index;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        if (CanWrite && _index != 0)
        {
            while (_index < BitsInByte)
            {
                UnsetBit(_buffer, _index++);
            }

            _baseStream.Position = _position;
            _baseStream.Write(_buffer);
            _position++;
            _index = 0;
        }
    }
    public override void SetLength(long value) => _baseStream.SetLength(value / BitsInByte);
    public override long Seek(long offset, SeekOrigin origin)
    {
        (long position, long index) = Math.DivRem(offset, BitsInByte);

        long newPosition = origin switch
        {
            SeekOrigin.Begin => position,
            SeekOrigin.Current => _position + position,
            SeekOrigin.End => Length + position,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin")
        };

        long newIndex = origin switch
        {
            SeekOrigin.Begin or SeekOrigin.End => index,
            SeekOrigin.Current => _index + index,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin")
        };

        if (newPosition < 0 || newPosition > Length)
            throw new IOException("Cannot seek to a given position");

        if (newIndex < 0 || newIndex > BitsInByte)
            throw new IOException("Cannot seek to a given index");

        _position = (int)newPosition;
        _index = (int)newIndex;
        return Position;
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length - offset, count);
        ArgumentOutOfRangeException.ThrowIfLessThan(Length - Position, count);

        int read = count;
        while (count > 0)
        {
            if (_index == 0)
            {
                _baseStream.Position = _position;
                _baseStream.Read(_buffer);
            }

            buffer[offset++] = (byte)(IsBitSet(_buffer, _index++) ? 1 : 0);
            count--;

            if (_index == BitsInByte)
            {
                _index = 0;
                _position++;
            }
        }

        return read;
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length - offset, count);

        if (Position + count > Length)
            count = (int)(Length - Position);

        while (count > 0)
        {
            if (buffer[offset++] != 0)
            {
                SetBit(_buffer, _index++);
            }
            else
            {
                UnsetBit(_buffer, _index++);
            }

            count--;

            if (_index == BitsInByte)
            {
                Flush();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        Flush();
        if (!_leaveOpen)
        {
            _baseStream.Dispose();
        }
    }
}