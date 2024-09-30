namespace Audio.Chunks.Types.HIRC;

[Flags]
public enum ChannelMask
{
    FrontLeft = 1 << 0,
    FrontRight = 1 << 1,
    FrontCenter = 1 << 2,
    LowFrequencyEffect = 1 << 3,
    BackLeft = 1 << 4,
    BackRight = 1 << 5,
    FrontLeftCenter = 1 << 6,
    FrontRightCenter = 1 << 7,
    BackCenter = 1 << 8,
    SideLeft = 1 << 9,
    SideRight = 1 << 10,
    TopCenter = 1 << 11,
    TopFrontLeft = 1 << 12,
    TopFrontCenter = 1 << 13,
    TopFrontRight = 1 << 14,
    TopBackLeft = 1 << 15,
    TopBackCenter = 1 << 16,
    TopBackRight = 1 << 17,
}