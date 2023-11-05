using Audio.Models.Entries;
using System.Collections.Generic;
using System.IO;

namespace Audio.Models.Chunks;
public record DIDX : Chunk
{
    public List<EmbeddedSound> EmbeddedSounds { get; set; }

    public DIDX(Chunk chunk) : base(chunk)
    {
        EmbeddedSounds = new List<EmbeddedSound>();
    }

    public new void Parse(BinaryReader reader)
    {
        var pos = reader.BaseStream.Position;
        while (reader.BaseStream.Position - pos < Length)
        {
            var embeddedSounds = new EmbeddedSound();
            embeddedSounds.Parse(reader);
            EmbeddedSounds.Add(embeddedSounds);
        }
    }
}