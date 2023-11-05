using System.IO;
using System.Text;

namespace Audio.Models.Entries;
public record Folder
{
    public uint ID { get; set; }
    public uint Offset { get; set; }
    public string Name { get; set; }

    public static Folder Parse(BinaryReader reader, long baseOffset)
    {
        var folder = new Folder();

        folder.Offset = reader.ReadUInt32();
        folder.ID = reader.ReadUInt32();

        var curOffset = reader.BaseStream.Position;

        reader.BaseStream.Seek(baseOffset + folder.Offset, SeekOrigin.Begin);

        var sb = new StringBuilder();
        while (reader.PeekChar() != '\0')
        {
            sb.Append(reader.ReadChar());
            reader.ReadChar();
        }

        folder.Name = sb.ToString();

        reader.BaseStream.Position = curOffset;

        return folder;
    }
}
