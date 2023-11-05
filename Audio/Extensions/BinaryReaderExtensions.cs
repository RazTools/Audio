using System.IO;
using System.Text;

namespace Audio.Extensions;
public static class BinaryReaderExtensions
{
    public static string ReadRawString(this BinaryReader reader, int length)
    {
        if (reader.BaseStream.Position + length > reader.BaseStream.Length)
        {
            throw new System.Exception($"String size {length} is out of bound !!");
        }
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }
}
