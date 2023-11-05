using System.IO;
using Audio.Models.Chunks;

namespace Audio.Models.Entries;
public record External : Entry
{
    public override string Location => Name.Contains("External_") ? $"{FolderName}/{Name}.wem" : Name + (Path.GetExtension(Name) == ".wem" ? "" : ".wem");
    public External() : base(EntryType.External) { }
    public static External Parse(BinaryReader reader, Package package)
    {
        var external = new External() { Package = package };

        external.ID = reader.ReadUInt64();
        var offsetMultiplier = reader.ReadUInt32();
        external.Size = reader.ReadUInt32();
        external.Offset = reader.ReadUInt32() * offsetMultiplier;
        external.Name = $"External_{external.ID}";
        external.Folder = reader.ReadUInt32();

        return external;
    }
}
