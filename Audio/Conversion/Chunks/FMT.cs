using Audio.Extensions;

namespace Audio.Conversion.Chunks;

public record FMT : WAVEChunk
{
    public new const string Signature = "fmt ";

    public WAVEFormat Format { get; set; }
    public ushort Channels { get; set; }
    public uint SampleRate { get; set; }
    public uint AverageBitrate { get; set; }
    public ushort BlockSize { get; set; }
    public ushort BitsPerSample { get; set; }
    public ushort ExtensionLength { get; set; }
    public ChannelType ChannelType { get; set; }
    public uint ChannelLayout { get; set; }

    public FMT(HeaderInfo header) : base(header) { }

    public override void Read(BinaryReader reader)
    {
        Format = (WAVEFormat)reader.ReadUInt16();
        Channels = reader.ReadUInt16();
        SampleRate = reader.ReadUInt32();
        AverageBitrate = reader.ReadUInt32();
        BlockSize = reader.ReadUInt16();
        BitsPerSample = reader.ReadUInt16();

        if (Header.Length > 0x10)
        {
            ExtensionLength = reader.ReadUInt16();
            reader.Align();
        }

        if (ExtensionLength > 0x06)
        {
            uint value = reader.ReadUInt32();

            int channelCount = (int)(value & 0xFF);
            if (channelCount == Channels)
            {
                ChannelType = (ChannelType)(value >> 8) & ChannelType.Mask;
                ChannelLayout = value >> 12;
            }
        }

        if (Format == WAVEFormat.IMA && BlockSize == 0x104 * Channels)
        {
            Format = WAVEFormat.PTADPCM;
        }
    }
}
