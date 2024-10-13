using Audio.Chunks;
using Audio.Conversion;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Audio.Entries;
public abstract record Entry : IBankReadable
{
    [JsonIgnore]
    public Chunk? Parent { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }
    public uint Folder { get; set; }
    public EntryType Type { get; set; }
    public string Source => Parent?.Source ?? "";
    public virtual string? Name { get; set; }
    public virtual string? Location => $"{FolderName}/{Name}";
    public virtual string FolderName => (Parent is AKPK akpk && akpk.FoldersDict.TryGetValue(Folder, out string? name) == true) ? name : "None";

    public Entry(EntryType type)
    {
        Type = type;
    }

    public virtual void Read(BankReader reader)
    {
        uint offsetMultiplier = reader.ReadUInt32();
        Size = reader.ReadUInt32();
        Offset = reader.ReadUInt32() * offsetMultiplier;
        Folder = reader.ReadUInt32();
    }
    public virtual bool TryOpen([NotNullWhen(true)] out Stream? stream)
    {
        if (Size > 0 && File.Exists(Source))
        {
            try
            {
                stream = File.OpenRead(Source);
                stream.Position = Offset;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Error while opening entry {Location}, {e}");
            }
        }

        stream = null;
        return false;
    }
    public virtual bool TryWrite(Stream outStream)
    {
        if (outStream.CanWrite && TryOpen(out Stream? inputStream))
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)Size);
            Span<byte> bufferSpan = buffer.AsSpan(0, (int)Size);

            try
            {
                inputStream.ReadExactly(bufferSpan);

                outStream.Write(bufferSpan);
                outStream.Position = 0;

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Error while writing entry {Location} to stream, {e}");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        return false;
    }
    public bool TryConvert(Stream outStream, [NotNullWhen(true)] out WWiseRIFFFile? audioFile)
    {
        try
        {
            using MemoryStream ms = new();
            if (Type is not EntryType.Bank && TryWrite(ms) && WWiseRIFFFile.TryParse(ms, out audioFile))
            {
                return audioFile.TryWrite(outStream);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error while converting audio file {Name}, {e}");
        }

        audioFile = null;
        return false;
    }
    public virtual bool TryDump(string outputDirectory, bool convert, [NotNullWhen(true)] out string? location)
    {
        location = Location;

        if (!string.IsNullOrEmpty(location))
        {
            string outputPath = Path.Combine(outputDirectory, location);
            string? dirPath = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            
            if (convert)
            {
                using MemoryStream memoryStream = new();
                if (TryConvert(memoryStream, out WWiseRIFFFile? audioFile) && !string.IsNullOrEmpty(audioFile.Extension))
                {
                    location = Path.ChangeExtension(location, audioFile.Extension);
                    outputPath = Path.Combine(outputDirectory, location);
                }

                if (File.Exists(outputPath))
                {
                    return true;
                }

                using FileStream fileStream = File.OpenWrite(outputPath);
                memoryStream.CopyTo(fileStream);
                return true;
            }
            else
            {
                if (File.Exists(outputPath))
                {
                    return true;
                }

                using FileStream fileStream = File.OpenWrite(outputPath);
                return TryWrite(fileStream);
            }
        }

        return false;
    }
}