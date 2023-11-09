using System.IO;
using System.Text;

namespace Audio.Models.Entries;
public record Folder
{
    public uint ID { get; set; }
    public uint Offset { get; set; }
    public string Name { get; set; }
    public bool IsChecked { get; set; }

    public static Folder Parse(BinaryReader reader, long baseOffset)
    {
        var folder = new Folder();

        folder.Offset = reader.ReadUInt32();
        folder.ID = reader.ReadUInt32();

        var curOffset = reader.BaseStream.Position;

        reader.BaseStream.Seek(baseOffset + folder.Offset, SeekOrigin.Begin);

        var isUTF16 = true;
        var sb = new StringBuilder();
        while (reader.PeekChar() != '\0')
        {
            sb.Append(reader.ReadChar());
            if (isUTF16 && reader.PeekChar() != '\0')
            {
                isUTF16 = false;
            }

            if (isUTF16)
            {
                reader.ReadChar();
            }
        }

        folder.Name = sb.ToString();

        reader.BaseStream.Position = curOffset;

        return folder;
    }
}
