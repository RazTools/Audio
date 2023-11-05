using System.IO;

namespace Audio.Models.Chunks;
public record BKHD : Chunk
{
    public uint Version { get; set; }
    public uint ID { get; set; }

    public BKHD(Chunk chunk) : base(chunk) { }

    public new void Parse(BinaryReader reader)
    {
        var pos = reader.BaseStream.Position;

        Version = reader.ReadUInt32();
        ID = reader.ReadUInt32();

        reader.BaseStream.Position = pos + Length;
    }
}
