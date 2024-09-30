using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record AdvancedSetting : IBankReadable
{
    public bool KillNewest { get; set; }
    public bool UseVirtualBehavior { get; set; }
    public bool IgnoreParentMaxNumInst { get; set; }
    public bool IsVVoicesOptOverrideParent { get; set; }
    public VirtualQueueBehavior VirtualQueueBehavior { get; set; }
    public ushort MaxNumInstance { get; set; }
    public BelowThresholdBehavior BelowThresholdBehavior { get; set; }
    public bool OverrideHdrEnvelope { get; set; }
    public bool OverrideAnalysis { get; set; }
    public bool NormalizeLoudness { get; set; }
    public bool EnableEnvelope { get; set; }

    public void Read(BankReader reader)
    {
        BitVector32 vector = new(reader.ReadByte());

        KillNewest = vector.Get(0);
        UseVirtualBehavior = vector.Get(1);
        IgnoreParentMaxNumInst = vector.Get(3);
        IsVVoicesOptOverrideParent = vector.Get(4);

        VirtualQueueBehavior = (VirtualQueueBehavior)reader.ReadByte();
        MaxNumInstance = reader.ReadUInt16();
        BelowThresholdBehavior = (BelowThresholdBehavior)reader.ReadByte();

        vector = new(reader.ReadByte());

        OverrideHdrEnvelope = vector.Get(0);
        OverrideAnalysis = vector.Get(1);
        NormalizeLoudness = vector.Get(2);
        EnableEnvelope = vector.Get(3);
    }
}