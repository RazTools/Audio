using Audio.Chunks;

namespace Audio.Entries;
public record Bank : TaggedEntry<uint>
{
    public BKHD? BKHD { get; set; }
    public override string? Location => $"{base.Location}.bnk";

    public Bank() : base(EntryType.Bank) { }

    public void Parse(AKPK parent)
    {
        using MemoryStream stream = new();
        if (TryWrite(stream))
        {
            using BankReader reader = new(stream);

            HeaderInfo header = new();
            header.Read(reader);

            BKHD = new BKHD(header) { Parent = parent };
            BKHD.Read(reader);

            header.Align(reader);
        }
    }
}
