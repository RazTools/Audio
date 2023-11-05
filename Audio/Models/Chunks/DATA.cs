using System.IO;

namespace Audio.Models.Chunks;
public record DATA : Chunk
{
    public uint Offset { get; set; }
    public DATA(Chunk chunk) : base(chunk) { }

    public new void Parse(BinaryReader reader)
    {
        Offset = (uint)reader.BaseStream.Position;

        reader.BaseStream.Position = Offset + Length; 
    }
}