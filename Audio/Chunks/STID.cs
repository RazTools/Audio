using Audio.Extensions;

namespace Audio.Chunks;
public record STID : Chunk
{
    public new const string Signature = "STID";

    public uint StringType { get; set; }

    public STID(HeaderInfo header) : base(header) { }

    public override void Read(BankReader reader)
    {
        StringType = reader.ReadUInt32();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            reader.ReadUInt32(); // ID
            byte nameLength = reader.ReadByte();
            string name = reader.ReadRawString(nameLength);
            FNVID<uint>.TryMatch(name, out _);
        }
    }
}