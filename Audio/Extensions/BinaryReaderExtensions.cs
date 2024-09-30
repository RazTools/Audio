using System.Text;

namespace Audio.Extensions;
public static class BinaryReaderExtensions
{
    public static string ReadRawString(this BinaryReader reader, int length)
    {
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }
    public static void Align(this BinaryReader reader, int alignment = 4)
    {
        long remainder = reader.BaseStream.Position % alignment;
        if (remainder != 0)
        {
            reader.BaseStream.Position += alignment - remainder;
        }
    }
}
