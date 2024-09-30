using Audio.Conversion.Chunks;
using System.Diagnostics.CodeAnalysis;

namespace Audio.Conversion;

public abstract record WAVEChunk : IReadable<BinaryReader>
{
    public const string Signature = "WAVE";

    private readonly static Dictionary<string, Func<HeaderInfo, WAVEChunk>> s_chunks = new()
    {
        { FMT.Signature, header => new FMT(header) },
        { VORB.Signature, header => new VORB(header) },
        { JUNK.Signature, header => new JUNK(header) },
        { DATA.Signature, header => new DATA(header) },
    };

    public HeaderInfo Header { get; private set; }

    public WAVEChunk(HeaderInfo header)
    {
        Header = header;
    }

    public abstract void Read(BinaryReader reader);

    public static bool TryParse(BinaryReader reader, [MaybeNullWhen(false)] out WAVEChunk chunk)
    {
        chunk = null;

        HeaderInfo header = new();
        header.Read(reader);

        if (s_chunks.TryGetValue(header.Signature, out Func<HeaderInfo, WAVEChunk>? waveChunkAction))
        {
            chunk = waveChunkAction(header);
            chunk.Read(reader);
        }

        header.Align(reader);
        return chunk != null;
    }
}