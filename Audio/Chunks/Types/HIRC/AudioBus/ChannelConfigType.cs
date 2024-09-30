namespace Audio.Chunks.Types.HIRC;

public enum ChannelConfigType
{
    Anonymous,
    Standard,
    Ambisonic,
    Objects,
    UseDeviceMain = 0xE,
    UseDevicePassthrough
}