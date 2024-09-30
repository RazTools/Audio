using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record BusEffect : IBankReadable
{
    public BitVector32 EffectBypass { get; set; }
    public EffectChunk[] Chunks { get; set; } = [];
    public uint RootID { get; set; }
    public bool RootIsShareSet { get; set; }

    public BusEffect()
    {
        EffectBypass = new(0);
    }

    public void Read(BankReader reader)
    {
        int effectCount = reader.ReadByte();

        if (effectCount > 0)
        {
            EffectBypass = new(reader.ReadByte());
            Chunks = new EffectChunk[effectCount];
            for (int i = 0; i < effectCount; i++)
            {
                Chunks[i] = new();
                Chunks[i].Read(reader);
            }
        }

        RootID = reader.ReadUInt32();
        RootIsShareSet = reader.ReadByte() != 0;
    }
}