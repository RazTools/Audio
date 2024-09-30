using Audio.Entries;

namespace Audio.Chunks;
public record DIDX : Chunk
{
    public new const string Signature = "DIDX";

    public List<EmbeddedSound> EmbeddedSounds { get; set; }

    public DIDX(HeaderInfo header) : base(header)
    {
        EmbeddedSounds = [];
    }

    public override void Read(BankReader reader)
    {
        long pos = reader.BaseStream.Position;
        while (reader.BaseStream.Position - pos < Header.Length)
        {
            EmbeddedSound embeddedSounds = new();
            embeddedSounds.Read(reader);
            EmbeddedSounds.Add(embeddedSounds);
        }
    }
}