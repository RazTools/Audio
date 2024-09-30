using Audio.Extensions;
using System.Collections.Specialized;

namespace Audio.Chunks.Types.HIRC;

public record BaseParameter : IBankReadable
{
    public BaseEffect Effect { get; set; }
    public byte OverrideAttachmentParams { get; set; }
    public FNVID<uint> OverrideBusId { get; set; }
    public FNVID<uint> DirectParentID { get; set; }
    public bool PriorityOverrideParent { get; set; }
    public bool PriorityApplyDistFactor { get; set; }
    public bool OverrideMidiEventsBehavior { get; set; }
    public bool OverrideMidiNoteTracking { get; set; }
    public bool EnableMidiNoteTracking { get; set; }
    public bool IsMidiBreakLoopOnNoteOff { get; set; }
    public ActionProperty[] Properties { get; set; } = [];
    public ActionModifier[] Modifiers { get; set; } = [];
    public Position Position { get; set; }
    public Auxiliary Auxiliary { get; set; }
    public AdvancedSetting AdvancedSettings { get; set; }
    public StateChunk StateChunk { get; set; }
    public RTPC RTPC { get; set; }

    public BaseParameter()
    {
        OverrideBusId = 0;
        DirectParentID = 0;
        Effect = new();
        Position = new();
        Auxiliary = new();
        AdvancedSettings = new();
        StateChunk = new();
        RTPC = new();
    }

    public void Read(BankReader reader)
    {
        Effect.Read(reader);
        OverrideAttachmentParams = reader.ReadByte();
        OverrideBusId = reader.ReadUInt32();
        DirectParentID = reader.ReadUInt32();

        BitVector32 vector = new(reader.ReadByte());

        PriorityOverrideParent = vector.Get(0);
        PriorityApplyDistFactor = vector.Get(1);
        OverrideMidiEventsBehavior = vector.Get(2);
        OverrideMidiNoteTracking = vector.Get(3);
        EnableMidiNoteTracking = vector.Get(4);
        IsMidiBreakLoopOnNoteOff = vector.Get(5);

        int propertyCount = reader.ReadByte();
        Properties = new ActionProperty[propertyCount];
        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i] = new();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].ID = (PropertyID)reader.ReadByte();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].Value = reader.ReadUInt32();
        }

        int modifierCount = reader.ReadByte();
        Modifiers = new ActionModifier[modifierCount];
        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i] = new();
        }

        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i].ID = (PropertyID)reader.ReadByte();
        }

        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i].Min = reader.ReadUInt32();
            Modifiers[i].Max = reader.ReadUInt32();
        }

        Position.Read(reader);
        Auxiliary.Read(reader);
        AdvancedSettings.Read(reader);
        StateChunk.Read(reader);
        RTPC.Read(reader);
    }
}