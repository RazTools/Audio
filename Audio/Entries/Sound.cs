namespace Audio.Entries;
public record Sound : TaggedEntry<uint>
{
    public override string? Location => $"{base.Location}.wem";

    public Sound() : base(EntryType.Sound) { }
}