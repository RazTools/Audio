using System.Diagnostics.CodeAnalysis;

namespace Audio.Chunks;
public abstract record Chunk : IBankReadable
{
    public const string Signature = "";

    private readonly static Dictionary<string, Func<HeaderInfo, Chunk>> s_chunks = new()
    {
        { AKPK.Signature, header => new AKPK(header) },
        { BKHD.Signature, header => new BKHD(header) },
        { HIRC.Signature, header => new HIRC(header) },
        { STMG.Signature, header => new STMG(header) },
        { STID.Signature, header => new STID(header) },
        { DIDX.Signature, header => new DIDX(header) },
        { DATA.Signature, header => new DATA(header) },
    };
    public Chunk? Parent { get; set; }
    public string? Source { get; set; }
    public HeaderInfo Header { get; private set; }

    public Chunk(HeaderInfo header)
    {
        Header = header;
    }  

    public abstract void Read(BankReader reader);

    public static bool TryParse(BankReader reader, [MaybeNullWhen(false)] out Chunk chunk)
    {
        chunk = null;

        HeaderInfo header = new();
        header.Read(reader);

        if (s_chunks.TryGetValue(header.Signature, out Func<HeaderInfo, Chunk>? chunkAction))
        {
            chunk = chunkAction(header);
            chunk.Read(reader);
        }

        header.Align(reader);
        return chunk != null;
    }
}
