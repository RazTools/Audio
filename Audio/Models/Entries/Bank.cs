using System.Collections.Generic;
using System.IO;
using Audio.Models.Chunks;

namespace Audio.Models.Entries;
public record Bank : Entry
{
    public Dictionary<string, Chunk> Chunks { get; set; }
    public override string Location => $"{base.Location}.bnk";
    public Bank() : base(EntryType.Bank)
    {
        Chunks = new Dictionary<string, Chunk>();
    }
    public List<EmbeddedSound> EmbeddedSounds
    {
        get
        {
            if (Chunks.TryGetValue("DIDX", out var chunk) && chunk is DIDX didx)
            {
                if (Chunks.TryGetValue("DATA", out chunk) && chunk is DATA data)
                {
                    foreach(var embeddedSound in didx.EmbeddedSounds)
                    {
                        embeddedSound.SetParent(this); 
                        embeddedSound.Offset += data.Offset;
                    }
                    return didx.EmbeddedSounds;
                }
            }
            return new List<EmbeddedSound>();
        }
    }
    public Dictionary<uint, string> BankIDToName
    {
        get
        {
            if (Chunks.TryGetValue("STID", out var chunk) && chunk is STID stid)
            {
                return stid.BankIDToName;
            }
            return new Dictionary<uint, string>();
        }
    }

    public static Bank Parse(BinaryReader reader, Package package)
    {
        var bank = new Bank() { Package = package };

        bank.ID = reader.ReadUInt32();
        var offsetMultiplier = reader.ReadUInt32();
        bank.Size = reader.ReadUInt32();
        bank.Offset = reader.ReadUInt32() * offsetMultiplier;
        bank.Name = $"Bank_{bank.ID}";
        bank.Folder = reader.ReadUInt32();

        return bank;
    }

    public void ParseChunks()
    {
        var bytes = GetData();

        using var ms = new MemoryStream(bytes);
        using var reader = new BinaryReader(ms);
        while (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            var chunk = Chunk.ParseChunk(reader);
            Chunks.Add(chunk.Signature, chunk);
        }
    }
}
