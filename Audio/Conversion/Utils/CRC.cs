namespace Audio.Conversion.Utils;
public class CRC
{
    private static readonly uint[] Table;

    static CRC()
    {
        Table = new uint[256];
        const uint kPoly = 0x04C11DB7;
        for (uint i = 0; i < 256; i++)
        {
            uint r = i << 24;
            for (int j = 0; j < 8; j++)
            {
                if ((r & 0x80000000) != 0)
                    r = r << 1 ^ kPoly;
                else
                    r <<= 1;
            }
            Table[i] = r;
        }
    }

    uint _value = 0;

    public void Init() { _value = 0; }
    public uint GetDigest() { return _value; }
    public void Update(byte[] data, int offset, int size) => Update(data.AsSpan(offset, size));
    public void Update(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i++)
            _value = Table[(byte)(_value >> 24) ^ data[i]] ^ _value << 8;
    }

    public static uint CalculateDigest(byte[] data, int offset, int size)
    {
        CRC crc = new();
        crc.Update(data, offset, size);
        return crc.GetDigest();
    }

    public static uint CalculateDigest(Span<byte> data)
    {
        CRC crc = new();
        crc.Update(data);
        return crc.GetDigest();
    }
}

