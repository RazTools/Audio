namespace Audio.Conversion.Chunks;

public enum WAVEFormat
{
    None,
    PCM,
    IMA,
    OPUS = 0x3040,
    OPUSWW = 0x3041,
    PTADPCM = 0x8311,
    AAC = 0xAAC0,
    VORBIS = 0xFFFF,
}