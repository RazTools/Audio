using Audio.Chunks;

namespace Audio.Entries;
public record EmbeddedSound : TaggedEntry<uint>
{
    private Bank? _bank = null;
    
    public override string? Location => $"{base.Location}.wem";
    public override string FolderName => $"{base.FolderName}/{Bank?.Name}";
    public Bank? Bank
    {
        get => _bank;
        set
        {
            if (value != null && value.BKHD?.GetChunk(out DATA? data) == true)
            {
                _bank = value;
                Folder = _bank.Folder;
                Parent = _bank.Parent;
                Offset += _bank.Offset + data.BaseOffset;
            }
        }
    }

    public EmbeddedSound() : base(EntryType.EmbeddedSound) { }

    public override void Read(BankReader reader)
    {
        ID = FNVID<uint>.Read(reader);
        Name = ID.ToString();
        Offset = reader.ReadUInt32();
        Size = reader.ReadUInt32();
    }
}