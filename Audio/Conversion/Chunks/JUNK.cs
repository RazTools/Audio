namespace Audio.Conversion.Chunks;
public record JUNK : WAVEChunk
{
    public new const string Signature = "JUNK";

    public JUNK(HeaderInfo header) : base(header) { }

    public override void Read(BinaryReader reader) { }
}
