using Audio.Conversion.Chunks;
using Audio.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Audio.Conversion;
public record WWiseRIFFHeader
{
    public const string Signature = "RIFF";

    private readonly Dictionary<Type, WAVEChunk> _chunks = [];
    private readonly Stream _stream;
    private readonly long _offset;

    public Stream Stream => _stream;
    public long Offset => _offset;

    public WWiseRIFFHeader(Stream stream)
    {
        _offset = stream.Position;
        _stream = stream;
    }

    public virtual void Parse()
    {
        using BinaryReader reader = new(_stream, Encoding.UTF8, true);
        reader.BaseStream.Position = _offset;

        string signature = reader.ReadRawString(4);
        if (signature != Signature)
        {
            throw new ArgumentException($"Invalid signature, Expected {Signature} got {signature}");
        }

        int size = reader.ReadInt32();
        string waveSignature = reader.ReadRawString(4);
        if (waveSignature != WAVEChunk.Signature)
        {
            throw new ArgumentException($"Invalid type signature, Expected {WAVEChunk.Signature} got {waveSignature}");
        }

        while (reader.BaseStream.Position < _offset + size)
        {
            if (WAVEChunk.TryParse(reader, out WAVEChunk? chunk))
            {
                _chunks.Add(chunk.GetType(), chunk);
            }
        }

        if (GetChunk(out FMT? fmt) && fmt.ExtensionLength == 0x30)
        {
            HeaderInfo vorbHeader = new()
            {
                Signature = VORB.Signature,
                Length = (uint)fmt.ExtensionLength - 6
            };

            vorbHeader.Offset = fmt.Header.Offset + fmt.Header.Length - vorbHeader.Length;

            reader.BaseStream.Position = vorbHeader.Offset;
            VORB vorb = new(vorbHeader);
            vorb.Read(reader);

            vorbHeader.Align(reader);
            _chunks.Add(typeof(VORB), vorb);
        }

        if (GetChunk(out DATA? data) && data.Header.Length > _stream.Length)
        {
            Logger.Warning($"Truncated audio stream, Expected {data.Header.Length} got {_stream.Length}, resizing...");
            data.Header.Length = (uint)(_stream.Length - data.Header.Offset);
        }
    }

    public bool GetChunk<T>([NotNullWhen(true)] out T? chunk) where T : WAVEChunk
    {
        if (_chunks.TryGetValue(typeof(T), out WAVEChunk? chk))
        {
            chunk = (T)chk;
            return true;
        }

        chunk = null;
        return false;
    }
}
