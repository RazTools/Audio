using System.Collections.Specialized;

namespace Audio.Extensions;
public static class BitVector32Extensions
{
    public static bool Get(this BitVector32 vector, int bitIndex) => vector[1 << bitIndex];
    public static T Mask<T>(this BitVector32 vector, int offset = 0) where T : struct, Enum
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (!Enum.TryParse("Mask", true, out T mask))
        {
            throw new Exception($"Mask not defined in enum {nameof(T)}");
        }

        short maxValue = (short)(int)(object)mask;
        BitVector32.Section section;

        if (offset == 0)
        {
            section = BitVector32.CreateSection(maxValue);
        }
        else
        {
            section = BitVector32.CreateSection(maxValue, BitVector32.CreateSection((short)((1 << offset) - 1)));
        }

        return (T)(object)vector[section];
    }
}
