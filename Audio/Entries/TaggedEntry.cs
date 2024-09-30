namespace Audio.Entries;
public abstract record TaggedEntry<T> : Entry where T : struct
{
    public FNVID<T> ID { get; set; }

    public TaggedEntry(EntryType type) : base(type)
    {
        ID = new();
    }

    public override void Read(BankReader reader)
    {
        ID = FNVID<T>.Read(reader);
        Name = ID.ToString();

        base.Read(reader);
    }
}
