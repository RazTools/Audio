using System.IO;
using Audio.Models.Chunks;

namespace Audio.Models.Entries;
public record Sound : Entry
{
    public override string Location => $"{base.Location}.wem";

    public Sound() : base(EntryType.Sound) { }

    public static Sound Parse(BinaryReader reader, Package package)
    {
        var sound = new Sound() { Package = package };

        sound.ID = reader.ReadUInt32();
        var offsetMultiplier = reader.ReadUInt32();
        sound.Size = reader.ReadUInt32();
        sound.Offset = reader.ReadUInt32() * offsetMultiplier;
        sound.Name = $"Sound_{sound.ID}";
        sound.Folder = reader.ReadUInt32();

        return sound;
    }
}