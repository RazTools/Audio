using System.IO;
using System.Text;

namespace Audio.Extensions;
public static class BinaryReaderExtensions
{
    public static string ReadRawString(this BinaryReader reader, int length)
    {
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }
}
