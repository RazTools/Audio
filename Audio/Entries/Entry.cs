using Audio.Chunks;
using Audio.Conversion;
using Audio.Utils;
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
    public string? Name { get; set; }
    public EntryType Type { get; set; }
    public string Source => Parent?.Source ?? "";
    public virtual string? Location => $"{FolderName}/{Name}";
    public virtual string FolderName => Parent is AKPK akpk && akpk.FoldersDict.TryGetValue(Folder, out string? name) == true ? name : "None";

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

                long pos = outStream.Position;
                outStream.Write(bufferSpan);
                outStream.Position = pos;

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
    public virtual bool TryDump(string outputDirectory, bool convert, [NotNullWhen(true)] out string? location)
    {
        location = Location;

        if (!string.IsNullOrEmpty(location))
        {
            WWiseRIFFFile? audioFile = null;

            string outputPath = Path.Combine(outputDirectory, location);
            string? dirPath = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (convert && TryParse(out audioFile))
            {
                if (!string.IsNullOrEmpty(audioFile.Extension))
                {
                    location = Path.ChangeExtension(location, audioFile.Extension);
                    outputPath = Path.Combine(outputDirectory, location);
                }
            }

            if (File.Exists(outputPath))
            {
                return true;
            }

            bool dumped;
            using (FileStream fileStream = File.OpenWrite(outputPath))
            {
                if (audioFile != null)
                {
                    dumped = audioFile.TryWrite(fileStream);
                    audioFile.Dispose();
                }
                else
                {
                    dumped = TryWrite(fileStream);
                }
            }

            if (dumped)
            {
                return true;
            }

            File.Delete(outputPath);
        }

        return false;
    }
    private bool TryParse([NotNullWhen(true)] out WWiseRIFFFile? audioFile)
    {
        try
        {
            MemoryStream ms = new();
            if (Type is not EntryType.Bank && TryWrite(ms) && WWiseRIFFFile.TryParse(ms, out audioFile))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error while parsing audio file {Name}, {e}");
        }

        audioFile = null;
        return false;
    }
}