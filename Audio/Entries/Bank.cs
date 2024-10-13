using Audio.Chunks;

namespace Audio.Entries;
public record Bank : TaggedEntry<uint>
{
    public BKHD? BKHD { get; set; }
    public override string? Location => $"{base.Location}.bnk";

    public IEnumerable<Entry> Entries
    {
        get
        {
            yield return this;

            foreach (EmbeddedSound embeddedSound in BKHD?.EmbeddedSounds ?? [])
            {
                embeddedSound.Bank ??= this;
                yield return embeddedSound;
            }
        }
    }

    public Bank() : base(EntryType.Bank) { }
    public Bank(BKHD bkhd) : this()
    {
        BKHD = bkhd;

        ID = bkhd.ID;
        Offset = 0;
        if (!string.IsNullOrEmpty(bkhd.Source) && File.Exists(bkhd.Source))
        {
            Size = (uint)new FileInfo(bkhd.Source).Length;
        }
    }

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
