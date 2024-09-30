namespace Audio.Conversion.Chunks;

public record LoopInfo : IReadable<BinaryReader>
{
    public uint LoopStartPacketOffset { get; set; }
    public uint LoopEndPacketOffset { get; set; }
    public ushort LoopBeginExtra { get; set; }
    public ushort LoopEndExtra { get; set; }

    public void Read(BinaryReader reader)
    {
        LoopStartPacketOffset = reader.ReadUInt32();
        LoopEndPacketOffset = reader.ReadUInt32();
        LoopBeginExtra = reader.ReadUInt16();
        LoopEndExtra = reader.ReadUInt16();
    }
}