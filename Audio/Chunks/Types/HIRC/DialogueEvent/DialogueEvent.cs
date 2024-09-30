using System.Runtime.InteropServices;

namespace Audio.Chunks.Types.HIRC;

public record DialogueEvent : HIRCObject
{
    public byte Probability { get; set; }
    public DecisionTree Tree { get; set; }
    public ActionProperty[] Properties { get; set; } = [];
    public ActionModifier[] Modifiers { get; set; } = [];

    public DialogueEvent(HeaderInfo header) : base(header)
    {
        Tree = new();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Probability = reader.ReadByte();
        Tree.Read(reader);

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
    }
}