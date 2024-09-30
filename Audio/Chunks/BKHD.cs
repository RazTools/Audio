using Audio.Entries;
using System.Diagnostics.CodeAnalysis;

namespace Audio.Chunks;
public record BKHD : Chunk
{
    public new const string Signature = "BKHD";

    private readonly Dictionary<Type, Chunk> _chunks = [];

    public uint Version { get; set; }
    public FNVID<uint> ID { get; set; }
    public FNVID<uint> LangaugeID { get; set; }
    public ushort Alignment { get; set; }

    public IEnumerable<EmbeddedSound> EmbeddedSounds
    {
        get
        {
            if (GetChunk(out DIDX? didx) == true)
            {
                foreach (EmbeddedSound embeddedSound in didx.EmbeddedSounds)
                {
                    yield return embeddedSound;
                }
            }
        }
    }

    public BKHD(HeaderInfo header) : base(header)
    {
        ID = 0;
        LangaugeID = 0;
    }

    public override void Read(BankReader reader)
    {
        Version = reader.ReadUInt32();
        ID = reader.ReadUInt32();
        LangaugeID = reader.ReadUInt32();
        Alignment = reader.ReadUInt16();
        Header.Align(reader);

        reader.Root = this;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            if (TryParse(reader, out Chunk? chunk))
            {
                chunk.Parent = this;
                _chunks.Add(chunk.GetType(), chunk);
            }
        }
    }

    public bool GetChunk<T>([NotNullWhen(true)] out T? chunk) where T : Chunk
    {
        if (_chunks.TryGetValue(typeof(T), out Chunk? chk))
        {
            chunk = (T)chk;
            return true;
        }

        chunk = null;
        return false;
    }
}