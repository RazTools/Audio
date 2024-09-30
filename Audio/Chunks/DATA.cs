namespace Audio.Chunks;
public record DATA : Chunk
{
    public new const string Signature = "DATA";

    public uint BaseOffset { get; set; }

    public DATA(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        BaseOffset = (uint)reader.BaseStream.Position;
    }
}