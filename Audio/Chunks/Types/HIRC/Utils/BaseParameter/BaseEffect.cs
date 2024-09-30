using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record BaseEffect : IBankReadable
{
    public bool IsOverrideParentFX { get; set; }
    public BitVector32 EffectBypass { get; set; }
    public EffectChunk[] Chunks { get; set; } = [];

    public BaseEffect()
    {
        EffectBypass = new(0);
    }

    public void Read(BankReader reader)
    {
        IsOverrideParentFX = reader.ReadByte() != 0;

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
    }
}