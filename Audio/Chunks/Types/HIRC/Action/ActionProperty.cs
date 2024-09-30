namespace Audio.Chunks.Types.HIRC;

public record ActionProperty
{
    public PropertyID ID { get; set; }
    public uint Value { get; set; }
}