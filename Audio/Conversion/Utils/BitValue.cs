using static Audio.Conversion.Utils.BitHelper;

namespace Audio.Conversion.Utils;
public struct BitValue
{
    private readonly int _bitCount;
    private uint _value;

    public BitValue(int count, uint value = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, sizeof(uint) * BitsInByte, nameof(count));

        _bitCount = count;

        for (int i = 0; i < _bitCount; i++)
        {
            _value |= value & (uint)(1 << i);
        }
    }

    public readonly byte[] Read()
    {
        byte[] buffer = new byte[_bitCount];

        for (int i = 0; i < _bitCount; i++)
        {
            if ((_value & (uint)(1 << i)) != 0)
            {
                buffer[i] = 1;
            }
        }

        return buffer;
    }

    public void Write(ReadOnlySpan<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, _bitCount, nameof(buffer));

        for (int i = 0; i < _bitCount; i++)
        {
            if (buffer[i] != 0)
            {
                _value |= (uint)(1 << i);
            }
            else
            {
                _value &= (uint)~(1 << i);
            }
        }
    }

    public override readonly string ToString() => $"{_value:X8}:{_bitCount}b";

    public static implicit operator int(BitValue value) => (int)value._value;
    public static implicit operator uint(BitValue value) => value._value;
}
