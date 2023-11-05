using System.IO;

namespace Audio.Models.Entries;
public record EmbeddedSound : Entry
{
    public Bank Bank { get; set; }
    public override string Location => $"{base.Location}.wem";
    public override string FolderName => $"{base.FolderName}/{Bank.Name}";

    public EmbeddedSound() : base(EntryType.EmbeddedSound) { }

    public void Parse(BinaryReader reader)
    {
        ID = reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        Size = reader.ReadUInt32();
        Name = $"EmbeddedSound_{ID}";
    }

    public void SetParent(Bank bank)
    {
        Bank = bank;
        Folder = bank.Folder;
        Package = bank.Package;
        Offset += bank.Offset;
    }
}