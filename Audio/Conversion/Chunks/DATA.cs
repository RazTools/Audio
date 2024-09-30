namespace Audio.Conversion.Chunks;
public record DATA : WAVEChunk
{
    public new const string Signature = "data";

    public uint BaseOffset { get; set; }

    public DATA(HeaderInfo header) : base(header) { }

    public override void Read(BinaryReader reader)
    {
        BaseOffset = (uint)reader.BaseStream.Position;
    }
}
