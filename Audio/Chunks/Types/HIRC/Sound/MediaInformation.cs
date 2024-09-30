using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record MediaInformation : IBankReadable
{
    public FNVID<uint> ID { get; set; }
    public uint Length { get; set; }
    public bool IsLanguageSpecific { get; set; }
    public bool Prefetch { get; set; }
    public bool NonCachable { get; set; }
    public bool HasSource { get; set; }

    public MediaInformation()
    {
        ID = 0;
    }

    public void Read(BankReader reader)
    {
        ID = reader.ReadUInt32();
        Length = reader.ReadUInt32();

        BitVector32 vector = new(reader.ReadByte());

        IsLanguageSpecific = vector.Get(0);
        Prefetch = vector.Get(1);
        NonCachable = vector.Get(3);
        HasSource = vector.Get(7);
    }
}