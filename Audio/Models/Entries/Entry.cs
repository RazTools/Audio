using System.IO;
using Audio.Models.Chunks;

namespace Audio.Models.Entries;
public record Entry
{
    public Package Package { get; set; }
    public ulong ID { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }
    public uint Folder { get; set; }
    public string Name { get; set; }
    public EntryType Type { get; }
    public string Source => Package.Path;
    public virtual string Location => $"{FolderName}/{Name}";
    public virtual string FolderName => Package.Folders[Folder];

    public Entry(EntryType type)
    {
        Type = type;
    }

    public byte[] GetData()
    {
        var bytes = new byte[Size];
        using var fs = File.OpenRead(Source);
        fs.Position = Offset;
        fs.Read(bytes);
        return bytes;
    }
}
