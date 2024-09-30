namespace Audio.Chunks.Types.HIRC;
public record State : HIRCObject
{
    public StateProperty[] Properties { get; set; }

    public State(HeaderInfo header) : base(header)
    {
        Properties = [];
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        int propertiesCount = reader.ReadInt16();
        Properties = new StateProperty[propertiesCount];
        for (int i = 0; i < propertiesCount; i++)
        {
            Properties[i] = new();
        }

        for (int i = 0; i < propertiesCount; i++)
        {
            Properties[i].ID = (StatePropertyID)reader.ReadUInt16();
        }

        for (int i = 0; i < propertiesCount; i++)
        {
            Properties[i].Value = reader.ReadSingle();
        }
    }
}