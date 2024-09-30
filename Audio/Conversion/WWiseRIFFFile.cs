using Audio.Conversion.Chunks;
using Audio.Conversion.Codecs;
using System.Diagnostics.CodeAnalysis;

namespace Audio.Conversion;
public class WWiseRIFFFile : IDisposable
{

    public virtual string Extension => ".wem";
    public WWiseRIFFHeader Header { get; private set; }

    public WWiseRIFFFile(WWiseRIFFHeader header)
    {
        Header = header;
    }

    public virtual bool TryWrite(Stream stream)
    {
        try
        {
            Header.Stream.Position = Header.Offset;
            Header.Stream.CopyTo(stream);

            return true;
        }
        catch(Exception e)
        {
            Logger.Warning($"Error while dumping RIFF file, {e}");
        }

        return false;
    }

    public static bool TryParse(Stream inputStream, [NotNullWhen(true)] out WWiseRIFFFile? audioFile)
    {
        WWiseRIFFHeader header = new(inputStream);
        header.Parse();

        if (header.GetChunk(out FMT? fmt))
        {
            audioFile = fmt.Format switch
            {
                WAVEFormat.VORBIS => new Vorbis(header),
                WAVEFormat.PTADPCM => new PTADPCM(header),
                _ => new WWiseRIFFFile(header),
            };

            return true;
        }

        audioFile = null;
        return false;
    }

    public void Dispose()
    {
        Header.Stream.Close();
        GC.SuppressFinalize(this);
    }
}

