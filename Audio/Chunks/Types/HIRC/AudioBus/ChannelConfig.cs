namespace Audio.Chunks.Types.HIRC;

public record ChannelConfig : IBankReadable
{
    public int ChannelCount { get; set; }
    public ChannelConfigType ConfigType { get; set; }
    public ChannelMask Mask { get; set; }

    public void Read(BankReader reader)
    {
        uint value = reader.ReadUInt32();

        ChannelCount = (int)value & 0xFF;
        ConfigType = (ChannelConfigType)(value >> 8) & ChannelConfigType.UseDevicePassthrough;
        Mask = (ChannelMask)(value >> 12 & 0xFFFFF);
    }
}