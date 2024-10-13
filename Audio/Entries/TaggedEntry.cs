namespace Audio.Entries;
public abstract record TaggedEntry<T> : Entry where T : struct
{
    public FNVID<T> ID { get; set; }
    public Dictionary<FNVID<T>, HashSet<EventTag>> Events { get; set; } = [];
    public override string? Name => ID.ToString();

    public TaggedEntry(EntryType type) : base(type)
    {
        ID = new();
    }

    public override void Read(BankReader reader)
    {
        ID = FNVID<T>.Read(reader);

        base.Read(reader);
    }
}
