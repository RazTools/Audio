using System.Numerics;

namespace Audio.Conversion.Utils;
public static class BitHelper
{
    public const int BitsInByte = 8;

    public static int ILog(uint value) => value == 0 ? 0 : BitOperations.Log2(value) + 1;

    public static void SetBit(Span<byte> buffer, int index)
    {
        (int byteIndex, int bitIndex) = Math.DivRem(index, BitsInByte);
        buffer[byteIndex] |= (byte)(1 << bitIndex);
    }

    public static void UnsetBit(Span<byte> buffer, int index)
    {
        (int byteIndex, int bitIndex) = Math.DivRem(index, BitsInByte);
        buffer[byteIndex] &= (byte)~(1 << bitIndex);
    }

    public static bool IsBitSet(ReadOnlySpan<byte> buffer, int index)
    {
        (int byteIndex, int bitIndex) = Math.DivRem(index, BitsInByte);
        return (buffer[byteIndex] & 1 << bitIndex) != 0;
    }

    public static BitValue Read(this BitStream bitStream, int bitCount)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, sizeof(uint) * BitsInByte, nameof(bitCount));

        Span<byte> buffer = stackalloc byte[bitCount];

        BitValue bitValue = new(bitCount);
        bitStream.Read(buffer);
        bitValue.Write(buffer);
        return bitValue;
    }

    public static void Write(this BitStream bitStream, BitValue value)
    {
        byte[] buffer = value.Read();
        bitStream.Write(buffer);
    }
}
