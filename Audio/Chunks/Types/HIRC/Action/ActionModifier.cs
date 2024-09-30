namespace Audio.Chunks.Types.HIRC;

public record ActionModifier
{
    public PropertyID ID { get; set; }
    public uint Min { get; set; }
    public uint Max { get; set; }
}