namespace Audio.Conversion.Chunks;
public record VORB : WAVEChunk
{
    public new const string Signature = "vorb";

    public uint TotalPCMFrames { get; set; }
    public LoopInfo LoopInfo { get; set; }
    public uint SeekTableSize { get; set; }
    public uint VorbisDataOffset { get; set; }
    public ushort MaxPacketSize { get; set; }
    public ushort LastGranuleExtra { get; set; }
    public uint DecodeAllocSize { get; set; }
    public uint DecodeX64AllocSize { get; set; }
    public uint HashCodebook { get; set; }
    public byte[] BlockSizes { get; set; }

    public VORB(HeaderInfo header) : base(header)
    {
        LoopInfo = new();
        BlockSizes = new byte[2];
    }

    public override void Read(BinaryReader reader)
    {
        TotalPCMFrames = reader.ReadUInt32();
        LoopInfo.Read(reader);
        SeekTableSize = reader.ReadUInt32();
        VorbisDataOffset = reader.ReadUInt32();
        MaxPacketSize = reader.ReadUInt16();
        LastGranuleExtra = reader.ReadUInt16();
        DecodeAllocSize = reader.ReadUInt32();
        DecodeX64AllocSize = reader.ReadUInt32();
        HashCodebook = reader.ReadUInt32();

        for (int i = 0; i < BlockSizes.Length; i++)
        {
            BlockSizes[i] = reader.ReadByte();
        }
    }
}
