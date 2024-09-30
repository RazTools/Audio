namespace Audio.Chunks.Types.HIRC;
public enum PluginType
{
    None,
    Codec,
    Source,
    Effect,
    MotionDevice,
    MotionSource,
    Mixer,
    Sink,
    GlobalExtension,
    Metadata,
    Mask = 0xF
}
