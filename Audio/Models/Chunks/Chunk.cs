using System.IO;
using Audio.Extensions;
using Audio.Models.Utils;

namespace Audio.Models.Chunks;
public record Chunk : IParse
{
    public string Signature { get; set; } = "";
    public uint Length { get; set; }

    public Chunk(Chunk chunk)
    {
        Signature = chunk.Signature;
        Length = chunk.Length;
    }
    
    public void Parse(BinaryReader reader)
    {
        Signature = reader.ReadRawString(4);
        Length = reader.ReadUInt32();
    }

    public static Chunk ParseChunk(BinaryReader reader)
    {
        var chunk = new Chunk();
        chunk.Parse(reader);

        switch (chunk.Signature)
        {
            case "AKPK":
                var package = new Package(chunk);
                package.Parse(reader);
                chunk = package;
                break;
            case "BKHD":  
                var bkhd = new BKHD(chunk);
                bkhd.Parse(reader);
                chunk = bkhd;
                break;
            case "STID":
                var stid = new STID(chunk);
                stid.Parse(reader);
                chunk = stid;
                break;
            case "DIDX":
                var didx = new DIDX(chunk);
                didx.Parse(reader);
                chunk = didx;
                break;
            case "DATA":
                var data = new DATA(chunk);
                data.Parse(reader);
                chunk = data;
                break;
            default:
                reader.BaseStream.Position += chunk.Length;
                break;
        }

        return chunk;
    }
}
